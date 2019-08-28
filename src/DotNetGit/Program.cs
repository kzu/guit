using System;
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
                App.Repository = new Repository(Directory.GetCurrentDirectory());
            }
            catch (RepositoryNotFoundException e)
            {
                var color = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(e.Message);
                Console.ForegroundColor = color;
                return;
            }

            Application.Init();
            Application.Run(new App());
        }
    }
}