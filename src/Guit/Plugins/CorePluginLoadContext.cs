using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Guit
{
    class CorePluginLoadContext : PluginLoadContext
    {
        readonly List<Assembly> assemblies;

        public CorePluginLoadContext(IEnumerable<string> corePlugins)
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            assemblies = corePlugins.Select(x => Default.LoadFromAssemblyPath(Path.Combine(baseDir, x))).ToList();
        }

        public override IEnumerable<Assembly> GetAssemblies() => assemblies;
    }
}