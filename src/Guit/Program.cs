using System;
using System.Collections.Generic;
using System.Composition.Hosting;
using Guit.Plugin.Changes;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit
{
    public static class Program
    {
        public static void Main()
        {
            try
            {
                var configuration = new ContainerConfiguration().WithAssembly(typeof(Program).Assembly);
                var container = configuration.CreateContainer();

                Application.Init();

                // Force a RepositoryNotFoundException up-front.
                container.GetExport<Repository>();
                // Force all singletons to be instantiated.
                container.GetExports<ISingleton>();

                //var app = container.GetExport<App>();
                
                Application.Run(container.GetExport<ChangesView>());
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