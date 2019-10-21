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
        static readonly HashSet<string> corePlugins = Assembly
            .GetExecutingAssembly()
            .GetCustomAttributes<CorePluginAttribute>()
            .Select(x => x.AssemblyFileName)
            .ToHashSet();

        readonly IRepository repository;

        [ImportingConstructor]
        public PluginManager(IRepository repository)
        {
            this.repository = repository;
        }

        public bool UseCorePlugins
        {
            get => repository.Config.Get<bool>("guit.coreplugins")?.Value ?? true;
            set
            {
                if (value)
                    // Since this is the default value, simply unset the config.
                    repository.Config.Unset("guit.coreplugins");
                else
                    repository.Config.Set("guit.coreplugins", false);
            }
        }

        public IEnumerable<PluginInfo> AvailablePlugins
        {
            get => repository.Config
                .OfType<ConfigurationEntry<string>>()
                .Where(x => x.Key == "guit.plugin")
                .Select(x => x.Value)
                .Concat(corePlugins)
                .Select(ReadPlugin)
                .Where(x => x.IsAvailable)
                .Distinct();
        }

        public IEnumerable<PluginInfo> EnabledPlugins
        {
            get => UseCorePlugins ? AvailablePlugins : 
                repository.Config
                    .OfType<ConfigurationEntry<string>>()
                    .Where(x => x.Key == "guit.plugin")
                    .Select(x => x.Value)
                    .Select(ReadPlugin)
                    .Where(x => x.IsAvailable)
                    .Distinct();
            set
            {
                if (corePlugins.All(id => value.Any(p => p.Id == id)))
                {
                    // If *all* plugins are included in the list, it basically means 
                    // opt-in to all core plugins. Set the appropriate config then.
                    UseCorePlugins = true;
                    // Also, in this case, we don't want/need to list each 
                    // individual plugin in the configuration, so keep it clean
                    value = value.Where(x => !corePlugins.Contains(x.Id));
                }
                else
                {
                    // If at least one plugin was missing (or all of them), 
                    // it's an opt-out of the implicit list of *all* plugins, 
                    // because we'll receive the specific list 
                    // of plugins (including potentially some core ones).
                    UseCorePlugins = false;
                }

                // Clear and persist new values.
                repository.Config.UnsetAll("guit.plugin");
                foreach (var plugin in value)
                {
                    repository.Config.Add("guit.plugin", plugin.Id);
                }
            }
        }

        public void Disable(string assemblyFile)
        {
            // Warn and disable the plugin for the next Load round.
            Console.WriteLine($"Disabling plugin {Path.GetFileName(assemblyFile)}...");
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
            var plugins = EnabledPlugins.ToList();

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
                        result = new PluginInfo(identity)
                        {
                            IsAvailable = true,
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
                result = new PluginInfo(identity)
                {
                    // TODO: check if plugin is currently enabled.
                    IsAvailable = true,
                    Title = parts[0],
                    Version = parts[1],
                };
            }

            if (result is null)
            {
                result = new PluginInfo(identity)
                {
                    IsAvailable = false,
                    Title = identity,
                    Version = "unknown",
                };
            }

            return result;
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
