using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using System.Xml;
using Microsoft.VisualStudio.Composition;
using Terminal.Gui;

namespace Guit
{
    public static class Program
    {
        public static void Main()
        {
            try
            {
                var plugins = new PluginManager(new RepositoryProvider().Repository).GetPlugins(Console.Out);

                // See https://github.com/microsoft/vs-mef/blob/master/doc/hosting.md
                var discovery = new AttributedPartDiscovery(Resolver.DefaultInstance, true);
                var catalog = ComposableCatalog.Create(Resolver.DefaultInstance);

                foreach (var plugin in plugins)
                {
                    //var name = AssemblyName.GetAssemblyName(plugin);
                    catalog = catalog.AddParts(discovery.CreatePartsAsync(Assembly.LoadFrom(plugin)).Result);
                }

                catalog = catalog
                    .AddParts(discovery.CreatePartsAsync(Assembly.GetExecutingAssembly()).Result)
                    .AddParts(discovery.CreatePartsAsync(typeof(IApp).Assembly).Result)
                    .WithCompositionService();

                var config = CompositionConfiguration.Create(catalog);
                if (!config.CompositionErrors.IsEmpty)
                {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    foreach (var error in config.CompositionErrors)
                        foreach (var diag in error)
                            Console.Out.WriteLine(diag.Message);

                    Console.ReadLine();
                }

                // Represents the container
                var provider = config.CreateExportProviderFactory().CreateExportProvider();

                Application.Init();

                // Force all singletons to be instantiated.
                provider.GetExportedValues<ISingleton>();

                AppDomain.CurrentDomain.UnhandledException += (sender, args) => Console.Error.WriteLine(args.ExceptionObject?.ToString());
                TaskScheduler.UnobservedTaskException += (sender, args) => Console.Error.WriteLine(args.Exception?.ToString());

                // Obtain our first exported value
                Application.Run(provider.GetExportedValue<App>());
            }
            catch (Exception e)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                if (e.InnerException != null)
                    Console.Error.WriteLine(e.InnerException.Message);
                else
                    Console.Error.WriteLine(e.Message);

                Console.ForegroundColor = color;
                return;
            }
        }
    }
}
