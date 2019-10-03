using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.Composition;

namespace Guit
{
    class CompositionManager
    {
        readonly PluginManager plugins;

        public CompositionManager(PluginManager plugins)
        {
            this.plugins = plugins;
        }

        public IComposition CreateComposition()
        {
            var context = plugins.Load();
            var assemblies = context.GetAssemblies();

            // See https://github.com/microsoft/vs-mef/blob/master/doc/hosting.md
            var discovery = new AttributedPartDiscovery(Resolver.DefaultInstance, true);
            var catalog = ComposableCatalog.Create(Resolver.DefaultInstance)
                .AddParts(discovery.CreatePartsAsync(Assembly.GetExecutingAssembly()).Result)
                .AddParts(discovery.CreatePartsAsync(typeof(IApp).Assembly).Result)
                .WithCompositionService();

            // Add parts from plugins
            foreach (var assembly in assemblies)
            {
                catalog = catalog.AddParts(discovery.CreatePartsAsync(assembly).Result);
            }

            foreach (var assemblyFile in catalog.DiscoveredParts.DiscoveryErrors.GroupBy(x => x.AssemblyPath).Select(x => x.Key))
            {
                plugins.Disable(assemblyFile);
            }
            
            var config = CompositionConfiguration.Create(catalog);

            foreach (var assembly in config.CompositionErrors
                .SelectMany(error => error
                .SelectMany(diagnostic => diagnostic.Parts
                .Select(part => part.Definition.Type.Assembly)))
                .Distinct())
            {
                plugins.Disable(assembly);
            }

            var provider = config.CreateExportProviderFactory().CreateExportProvider();

            return new Composition(provider, context);
        }

        class Composition : IComposition
        {
            ExportProvider exports;
            IDisposable context;

            public Composition(ExportProvider exports, IDisposable context)
            {
                this.exports = exports;
                this.context = context;
            }

            public void Dispose()
            {
                exports.Dispose();
                exports = null;
                // TODO: is this needed?
                GC.Collect();
                context.Dispose();
                context = null;
            }

            public T GetExport<T>() => exports.GetExportedValue<T>();

            public IEnumerable<T> GetExports<T>() => exports.GetExportedValues<T>();
        }
    }
}
