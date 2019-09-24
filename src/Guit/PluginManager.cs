using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using LibGit2Sharp;

namespace Guit
{
    class PluginManager
    {
        readonly IRepository repository;

        public PluginManager(IRepository repository)
        {
            this.repository = repository;
        }

        public IEnumerable<string> GetPlugins(TextWriter output)
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var plugins = repository.Config
                .OfType<ConfigurationEntry<string>>()
                .Where(x => x.Key == "guit.plugin")
                .Select(x => x.Value);

            var coreplugins = repository.Config.Get<bool>("guit.coreplugins")?.Value ?? true;
            if (coreplugins)
            {
                plugins = Assembly
                    .GetExecutingAssembly()
                    .GetCustomAttributes<PluginAttribute>()
                    .Select(x => x.AssemblyFileName)
                    .Concat(plugins);
            }

            plugins = plugins.Select(x => Path.IsPathRooted(x) ? x : Path.Combine(baseDir, x)).ToArray();
            foreach (var plugin in plugins)
            {
                output.WriteLine($"Found plugin {plugin}");
            }

            // TODO: when plugins have a version, we need to check the 
            // nuget cache, download if necessary, and only then return 
            // the full path to the assembly.

            return plugins;
        }
    }
}
