using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Guit
{
    class CorePluginLoadContext : PluginLoadContext
    {
        readonly IEnumerable<Assembly> assemblies;

        public CorePluginLoadContext(IEnumerable<string> corePlugins)
        {
            var baseDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            assemblies = baseDir != null ?
                corePlugins.Select(x => Default.LoadFromAssemblyPath(Path.Combine(baseDir, x))).ToList() :
                Enumerable.Empty<Assembly>();
        }

        public override IEnumerable<Assembly> GetAssemblies() => assemblies;
    }
}