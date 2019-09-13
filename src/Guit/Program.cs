using System;
using System.Reflection;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.VisualStudio.Composition;
using Terminal.Gui;

namespace Guit
{
    public static class Program
    {
        [MTAThread]
        public static async Task Main()
        {
            try
            {
                // See https://github.com/microsoft/vs-mef/blob/master/doc/hosting.md
                var discovery = new AttributedPartDiscovery(Resolver.DefaultInstance, true);
                var catalog = ComposableCatalog.Create(Resolver.DefaultInstance)
                    // TODO: pull plugins assemblies
                    .AddParts(await discovery.CreatePartsAsync(Assembly.GetExecutingAssembly()))
                    .WithCompositionService();

                var config = CompositionConfiguration.Create(catalog);
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
            catch (CompositionFailedException e)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                if (e.InnerException != null)
                    Console.Error.WriteLine(e.Message);
                else
                    Console.Error.WriteLine(e.ToString());

                Console.ForegroundColor = color;
                return;
            }
        }
    }
}
