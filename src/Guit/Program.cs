using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Terminal.Gui;
using System.Runtime.Versioning;
using System.Diagnostics;
using System.Threading;

namespace Guit
{
    public static class Program
    {
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) => WriteError(args.ExceptionObject?.ToString());
            TaskScheduler.UnobservedTaskException += (sender, args) => WriteError(args.Exception?.ToString());

            try
            {
                var repositoryProvider = new RepositoryProvider(Directory.GetCurrentDirectory());
                var pluginManager = new PluginManager(repositoryProvider.Repository);
                var compositionManager = new CompositionManager(pluginManager);

                Application.Init();
                Application.Run(new App(compositionManager));
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

                Console.ReadLine();
                return;
            }
        }

        static void WriteError(string? message)
        {
            var color = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine(message);
            Console.ForegroundColor = color;
        }

        static void RefreshPlugins(List<PluginInfo> plugins, string hash, string projectFile)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(projectFile));
            new XDocument(
                new XElement("Project", 
                    new XAttribute("Sdk", "Microsoft.NET.Sdk"), 
                    new XElement("PropertyGroup",
                        new XElement("AssemblyName", "Guit.Plugins"),
                        new XElement("Configuration", "Release"),
                        new XElement("TargetFramework", "netcoreapp" + 
                            new FrameworkName(
                                Assembly.GetEntryAssembly()?.GetCustomAttribute<TargetFrameworkAttribute>()?.FrameworkName ?? "Unknown")
                            .Version),
                        new XElement("PackageReferencesHash", hash)),
                    new XElement("ItemGroup", plugins
                        .Where(x => !x.Id.EndsWith(".dll"))
                        .Select(x => new XElement("PackageReference", 
                            new XAttribute("Include", x.Id), 
                            new XAttribute("Version", x.Version)))), 
                    new XElement("Import", new XAttribute("Project", "$(GuitPath)\\Guit.Plugins.targets"))
                )                            
            ).Save(projectFile);

            var psi = new ProcessStartInfo("dotnet", $"build \"{projectFile}\"")
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            psi.Environment["GuitPath"] = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            var ev = new ManualResetEventSlim();
            var dotnet = Process.Start(psi);
            dotnet.EnableRaisingEvents = true;
            dotnet.Exited += (_, __) => ev.Set();

            Console.Out.WriteLine(dotnet.StandardOutput.ReadToEnd());
            Console.Error.WriteLine(dotnet.StandardError.ReadToEnd());

            if (!dotnet.HasExited)
                ev.Wait();

            if (dotnet.ExitCode != 0)
                throw new ArgumentException("Failed to refresh configured plugins.");
        }

        static string GetPluginsHash(List<PluginInfo> plugins)
        {
            var sb = new StringBuilder();
            foreach (var plugin in plugins)
            {
                if (plugin.Id.EndsWith(".dll"))
                    sb.Append(plugin.Id);
                else
                    sb.Append(plugin.Id).Append(',').Append(plugin.Version);

                sb.Append(';');
            }

            using var sha1 = System.Security.Cryptography.SHA1.Create();

            return string.Join("", sha1
                .ComputeHash(Encoding.UTF8.GetBytes(sb.ToString()))
                .Select(b => b.ToString("X2")));
        }
    }
}
