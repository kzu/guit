using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Terminal.Gui;

namespace Guit
{
    class InstallPluginsDialog : DialogBox
    {
        readonly IPluginManager manager;

        ListView<IPackageSearchMetadata> plugins = new ListView<IPackageSearchMetadata>(
            new ColumnDefinition<IPackageSearchMetadata>(m => m.Identity.Id, 40),
            new ColumnDefinition<IPackageSearchMetadata>(m => m.Title, "*"))
        {
            AllowsMarking = true
        };

        bool done;

        public InstallPluginsDialog(IPluginManager manager) : base("Searching plugins...")
        {
            Buttons = DialogBoxButton.Ok;
            this.manager = manager;
        }

        public IEnumerable<IPackageSearchMetadata> GetMarkedEntries() 
            => plugins.Values.Where((x, i) => plugins.Source.IsMarked(i));

        protected override void EndInit()
        {
            Height = Dim.Fill(5);
            plugins.Width = Dim.Fill(1);
            plugins.Height = Dim.Height(this) - 6;

            Add(plugins);

            base.EndInit();

            Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(1), _ => UpdateProgress());
            Task.Run(async () => await LoadPlugins());
        }

        async Task LoadPlugins()
        {
            var providers = Repository.Provider.GetCoreV3();
            var source = new PackageSource("https://api.nuget.org/v3/index.json");
            var repo = new SourceRepository(source, providers);
            var search = await repo.GetResourceAsync<PackageSearchResource>();

            // replace with 'guit'
            var results = await search.SearchAsync("guit", new SearchFilter(true) { SupportedFrameworks = new[] { "netstandard2.0", "netcoreapp3.0" } }, 0, 35, new Logger(), CancellationToken.None);

            plugins.SetValues(results.Where(x => x.Identity.Id != "Guit.Core" && x.Tags.Contains("guit") && !manager.EnabledPlugins.Any(p => p.Id == x.Identity.Id)));
            // TODO: this does not result in the listview being properly focused/painted :(
            Application.MainLoop.Invoke(() => SetFocus(plugins));

            done = true;
        }

        bool UpdateProgress()
        {
            if (done)
            {
                Title = "Install Plugins";
                return false;
            }

            Title += ".";
            return true;
        }

        class Logger : ILogger
        {
            public void LogDebug(string data) => Console.WriteLine($"DEBUG: {data}");
            public void LogVerbose(string data) => Console.WriteLine($"VERBOSE: {data}");
            public void LogInformation(string data) => Console.WriteLine($"INFORMATION: {data}");
            public void LogMinimal(string data) => Console.WriteLine($"MINIMAL: {data}");
            public void LogWarning(string data) => Console.WriteLine($"WARNING: {data}");
            public void LogError(string data) => Console.WriteLine($"ERROR: {data}");
            public void LogErrorSummary(string data) => Console.WriteLine($"ERROR: {data}");
            public void LogSummary(string data) => Console.WriteLine($"SUMMARY: {data}");
            public void LogInformationSummary(string data) => Console.WriteLine($"SUMMARY: {data}");
        }
    }
}
