using System;
using System.Composition.Hosting;
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

                container.GetExport<App>().Run();
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