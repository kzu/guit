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

                AppDomain.CurrentDomain.UnhandledException += (sender, args) => Console.Error.WriteLine(args.ExceptionObject?.ToString());
                TaskScheduler.UnobservedTaskException += (sender, args) => Console.Error.WriteLine(args.Exception?.ToString());

                // Obtain our first exported value
                Application.Run(provider.GetExportedValue<App>());
            }
            catch (RepositoryNotFoundException e)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(e.Message);
                Console.ForegroundColor = color;
                return;
            }
        }
    }
}
