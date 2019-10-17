using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using System.Threading;
using LibGit2Sharp;

namespace Guit
{
    [Shared]
    [Export(typeof(IPluginManager))]
    class PluginManager : IPluginManager
    {
        static readonly Regex pluginVersionExpr = new Regex("<PluginVersion>(.*)</PluginVersion>", RegexOptions.Compiled);
        static readonly string[] corePlugins = Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes<CorePluginAttribute>()
            .Select(x => x.AssemblyFileName)
            .ToArray();

        readonly IRepository repository;

        [ImportingConstructor]
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
                repository.Config
                    .OfType<ConfigurationEntry<string>>()
                    .Where(x => x.Key == "guit.plugin")
                    .Select(x => x.Value)
                    .Concat(corePlugins)
                    .Select(ReadPlugin)
                    .Distinct() :
                repository.Config
                    .OfType<ConfigurationEntry<string>>()
                    .Where(x => x.Key == "guit.plugin")
                    .Select(x => x.Value)
                    .Select(ReadPlugin)
                    .Distinct();
            set
            {
                repository.Config.UnsetAll("guit.plugin");
                foreach (var plugin in value)
                {
                    repository.Config.Add("guit.plugin", plugin.Id);
                }
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
            var guitDir = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            var baseDir = Path.Combine(repository.Info.Path, "guit");

            var contexts = new List<PluginLoadContext>();
            var template = Path.Combine(guitDir, "Guit.Plugin.csproj");
            var plugins = Plugins.ToList();

            foreach (var plugin in plugins.Where(x => !x.Id.EndsWith(".dll")))
            {
                var pluginLib = "Guit.Plugin." + plugin.Id;
                var pluginDir = Path.Combine(baseDir, plugin.Id);
                var pluginProject = Path.Combine(pluginDir, pluginLib + ".csproj");
                var pluginReferences = Path.Combine(pluginDir, "obj", "ReferencePaths.txt");

                if (!File.Exists(pluginProject) || 
                    !File.Exists(Path.Combine(pluginDir, "bin", "Debug", pluginLib + ".dll")) ||
                    File.ReadLines(pluginProject)
                        .Select(line => pluginVersionExpr.Match(line))
                        .Where(m => m.Success)
                        .Select(m => m.Groups[1].Value)
                        .FirstOrDefault() != plugin.Version)
                {
                    Directory.CreateDirectory(pluginDir);
                    File.Copy(Path.Combine(guitDir, "Guit.Plugin.cs"), Path.Combine(pluginDir, "Program.cs"), true);
                    File.Copy(template, pluginProject, true);

                    File.WriteAllText(pluginProject, File.ReadAllText(pluginProject)
                        .Replace("$PluginId$", plugin.Id)
                        .Replace("$PluginVersion$", plugin.Version));

                    if (File.Exists(pluginReferences))
                        File.Delete(pluginReferences);
                }

                // If reference paths file exists and any of its referenced assemblies are 
                // not found, we need to perform a restore.
                if (!File.Exists(pluginReferences) || 
                    File.ReadLines(pluginReferences).Any(assemblyFile => !File.Exists(assemblyFile)))
                {
                    var psi = new ProcessStartInfo("dotnet")
                    {
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                    };

                    psi.ArgumentList.Add("msbuild");
                    psi.ArgumentList.Add("-r");
                    psi.ArgumentList.Add("-nologo");
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
                }
                else
                {
                    // We don't need to do anything!
                    Console.WriteLine($"Plugin {plugin.Id},{plugin.Version} is up-to-date");
                }

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
            var result = default(PluginInfo);

            if (identity.EndsWith(".dll"))
            {
                // This is a built-in plugin.
                var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (baseDir != null)
                {
                    var filePath = Path.Combine(baseDir, identity);
                    if (File.Exists(filePath))
                    {
                        var assembly = Assembly.Load(AssemblyName.GetAssemblyName(filePath));
                        result = new PluginInfo
                        {
                            IsAvailable = true,
                            Id = identity,
                            Title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title,
                            Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description,
                            Version = assembly.GetName().Version?.ToString(),
                        };
                    }
                }
            }
            else
            {
                var parts = identity.Split(',');
                result = new PluginInfo
                {
                    // TODO: check if plugin is currently enabled.
                    IsAvailable = true,
                    Id = identity,
                    Title = parts[0],
                    Version = parts[1],
                };
            }

            if (result is null)
            {
                result = new PluginInfo
                {
                    IsAvailable = false,
                    Id = identity,
                    Title = identity,
                    Version = "unknown",
                };
            }

            return result;
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
