using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Guit
{
    class NuGetPluginLoadContext : PluginLoadContext
    {
        readonly AssemblyDependencyResolver resolver;
        readonly AssemblyLoadContext parent;
        readonly string[] references;

        public NuGetPluginLoadContext(string id, string version, string path, string[] references, AssemblyLoadContext parent)
            : base(id + "," + version, true)
        {
            Id = id;
            Version = version;

            if (!File.Exists(path))
                throw new FileNotFoundException("Did not find specified plugin assembly file.", path);

            resolver = new AssemblyDependencyResolver(path);
            this.references = references;
            this.parent = parent;
        }

        public string Id { get; private set; }

        public string Version { get; private set; }

        public override IEnumerable<Assembly> GetAssemblies() => references.Select(x => Load(AssemblyName.GetAssemblyName(x)));

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // NOTE: we just match by simple name. We might want to throw/warn if plugin references older/newer version?
            var loadedAssembly = parent.Assemblies.FirstOrDefault(x => x.GetName().Name == assemblyName.Name);
            if (loadedAssembly != null)
                return loadedAssembly;

            // TODO: Reuse core assemblies from the main load context.
            var assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
                return LoadFromAssemblyPath(assemblyPath);

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
                return LoadUnmanagedDllFromPath(libraryPath);

            return IntPtr.Zero;
        }
    }
}
