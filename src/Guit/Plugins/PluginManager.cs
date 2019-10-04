using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using LibGit2Sharp;

namespace Guit
{
    class PluginManager : IPluginManager
    {
        static readonly string[] corePlugins = Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes<CorePluginAttribute>()
            .Select(x => x.AssemblyFileName)
            .ToArray();

        readonly IRepository repository;

        public PluginManager(IRepository repository)
        {
            this.repository = repository;
        }

        public bool UseCorePlugins 
        { 
            get => repository.Config.Get<bool>("guit.coreplugins")?.Value ?? true;
            set => repository.Config.Set("guit.coreplugins", value);
        }

        public IEnumerable<PluginInfo> Plugins 
        {
            get => UseCorePlugins ?
                repository.Config.GetValueOrDefault("guit.plugins", "")
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Concat(corePlugins)
                    .Select(ReadPlugin)
                    .Distinct() :
                repository.Config.GetValueOrDefault("guit.plugins", "")
                    .Split(';', StringSplitOptions.RemoveEmptyEntries)
                    .Select(ReadPlugin)
                    .Distinct();
            //get => UseCorePlugins ?
            //    repository.Config
            //        .OfType<ConfigurationEntry<string>>()
            //        .Where(x => x.Key == "guit.plugin")
            //        .Select(x => x.Value)
            //        .Concat(corePlugins)
            //        .Distinct() :
            //    repository.Config
            //        .OfType<ConfigurationEntry<string>>()
            //        .Where(x => x.Key == "guit.plugin")
            //        .Select(x => x.Value)
            //        .Distinct();
            set
            {
                //repository.Config
                //    .Set()
                //    .OfType<ConfigurationEntry<string>>()
                //    .Where(x => x.Key == "guit.plugin")
                //    .ToList().ForEach(x => x.)
                //    .Select(x => x.Value)
                //    .Concat(corePlugins)
                //    .Distinct() :
                
            }
        }

        public void Disable(string assemblyFile)
        {
        }

        public void Disable(Assembly assembly)
        {
            if (AssemblyLoadContext.GetLoadContext(assembly) is NuGetPluginLoadContext context)
            {
                // Warn and disable the plugin for the next Load round.
                Console.WriteLine($"Disabling plugin {context.Name}...");
            }
        }

        public IPluginContext Load()
        {
            var guitDir = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            var baseDir = Path.Combine(repository.Info.Path, "guit");

            var contexts = new List<PluginLoadContext>();
            var template = Path.Combine(guitDir, "Guit.Plugin.csproj");
            var plugins = Plugins.ToList();

            foreach (var plugin in plugins.Where(x => !x.Id.EndsWith(".dll")))
            {
                // TODO: skip if hash/version hasn't changed.
                var pluginLib = "Guit.Plugin." + plugin.Id;
                var pluginDir = Path.Combine(baseDir, plugin.Id);
                var pluginProject = Path.Combine(pluginDir, pluginLib + ".csproj");
                Directory.CreateDirectory(pluginDir);
                File.Copy(Path.Combine(guitDir, "Guit.Plugin.cs"), Path.Combine(pluginDir, "Program.cs"), true);
                File.Copy(template, pluginProject, true);
                File.WriteAllText(pluginProject, File.ReadAllText(pluginProject)
                    .Replace("$(PluginId)", plugin.Id)
                    .Replace("$(PluginVersion)", plugin.Version));

                var psi = new ProcessStartInfo("dotnet")
                {
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                };

                psi.ArgumentList.Add("msbuild");
                psi.ArgumentList.Add("-r");
                psi.ArgumentList.Add($"-bl:\"{Path.Combine(pluginDir, "msbuild.binlog")}\"");
                psi.ArgumentList.Add($"\"{pluginProject}\"");

                var ev = new ManualResetEventSlim();
                var dotnet = Process.Start(psi);
                dotnet.EnableRaisingEvents = true;
                dotnet.Exited += (_, __) => ev.Set();

                // TODO: how to surface this to the console in some other way?
                Console.Out.WriteLine(dotnet.StandardOutput.ReadToEnd());
                Console.Error.WriteLine(dotnet.StandardError.ReadToEnd());

                if (!dotnet.HasExited)
                    ev.Wait();

                if (dotnet.ExitCode != 0)
                    throw new ArgumentException("Failed to refresh configured plugins.");

                contexts.Add(new NuGetPluginLoadContext(
                    plugin.Id,
                    plugin.Version,
                    Path.Combine(pluginDir, "bin", "Debug", pluginLib + ".dll"),
                    File.ReadAllLines(Path.Combine(pluginDir, "obj", "ReferencePaths.txt")),
                    AssemblyLoadContext.Default));
            }

            contexts.Add(new CorePluginLoadContext(plugins.Where(x => x.Id.EndsWith(".dll")).Select(x => x.Id)));

            return new PluginContext(contexts);
        }

        private PluginInfo ReadPlugin(string identity)
        {
            if (identity.EndsWith(".dll"))
            {
                // This is a built-in plugin.
                var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var filePath = Path.Combine(baseDir, identity);
                if (File.Exists(filePath))
                {
                    var assembly = Assembly.Load(AssemblyName.GetAssemblyName(filePath));
                    return new PluginInfo
                    {
                        IsAvailable = true,
                        Id = identity,
                        Title = assembly.GetCustomAttribute<AssemblyTitleAttribute>().Title,
                        Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>().Description,
                        Version = assembly.GetName().Version.ToString(),
                    };
                }
                else
                {
                    return new PluginInfo
                    {
                        IsAvailable = false,
                        Id = identity, 
                        Title = identity,
                        Version = "unknown",
                    };
                }
            }
            else
            {
                var parts = identity.Split(',');
                return new PluginInfo
                {
                    // TODO: check if plugin is currently enabled.
                    IsAvailable = true,
                    Id = parts[0],
                    Title = parts[0],
                    Version = parts[1],
                };
            }
        }

        public IEnumerable<string> GetPlugins()
        {
            var plugins = (repository.Config.GetValueOrDefault("guit.plugins", ""))
                .Split(';');

            // Once the 
            //var plugins = repository.Config
            //    .OfType<ConfigurationEntry<string>>()
            //    .Where(x => x.Key == "guit.plugin")
            //    .Select(x => x.Value);

            var coreplugins = repository.Config.Get<bool>("guit.coreplugins")?.Value ?? true;
            if (coreplugins)
            {
                plugins = Assembly
                    .GetExecutingAssembly()
                    .GetCustomAttributes<CorePluginAttribute>()
                    .Select(x => x.AssemblyFileName)
                    .Concat(plugins)
                    .ToArray();
            }

            return plugins;
        }

        class PluginContext : IPluginContext
        {
            readonly List<PluginLoadContext> loadContexts;

            public PluginContext(List<PluginLoadContext> loadContexts) => this.loadContexts = loadContexts;

            public void Dispose()
            {
                foreach (var context in loadContexts.Where(x => x.IsCollectible))
                {
                    context.Unload();
                }
            }

            public IEnumerable<Assembly> GetAssemblies() => loadContexts.SelectMany(x => x.GetAssemblies());
        }
    }
}
