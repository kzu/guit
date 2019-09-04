using System;
using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.Composition.Hosting;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using LibGit2Sharp;
using Terminal.Gui;

namespace DotNetGit
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

                var app = container.GetExport<App>();

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