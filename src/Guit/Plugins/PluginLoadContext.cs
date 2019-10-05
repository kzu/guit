using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

namespace Guit
{
    abstract class PluginLoadContext : AssemblyLoadContext
    {
        protected PluginLoadContext(string? name = null, bool isCollectible = false) : base(name, isCollectible) { }

        public abstract IEnumerable<Assembly> GetAssemblies();
    }
}