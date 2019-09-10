using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using System.Threading.Tasks;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit
{
    public static class Program
    {
        [MTAThread]
        public static void Main()
        {
            try
            {
                var configuration = new ContainerConfiguration().WithAssembly(typeof(Program).Assembly);
                var container = configuration.CreateContainer();

                Application.Init();

                // Force a RepositoryNotFoundException up-front.
                container.GetExport<Repository>();

                var app = container.GetExport<App>();

                AppDomain.CurrentDomain.UnhandledException += (sender, args) => Console.Error.WriteLine(args.ExceptionObject?.ToString());
                TaskScheduler.UnobservedTaskException += (sender, args) => Console.Error.WriteLine(args.Exception?.ToString());

                Application.Run(app);
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
