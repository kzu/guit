using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Terminal.Gui;

namespace Guit
{
    [Export]
    [Export(typeof(IApp))]
    [Shared]
    class App : Toplevel, IApp
    {
        readonly ConcurrentDictionary<ContentView, string> contexts = new ConcurrentDictionary<ContentView, string>();
        readonly ConcurrentDictionary<ContentView, ShellWindow> shellWindows = new ConcurrentDictionary<ContentView, ShellWindow>();

        readonly Lazy<ContentView, MenuCommandMetadata> defaultView;
        readonly IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views;

        readonly MainThread mainThread;
        readonly Lazy<CommandService> commandService;

        [ImportingConstructor]
        public App(
            [ImportMany] IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views,
            MainThread mainThread,
            Lazy<CommandService> commandService)
        {
            this.views = views
                .OrderBy(x => x.Metadata.Order)
                .ThenBy(x => x.Metadata.Key);

            this.defaultView = this.views.First();

            this.mainThread = mainThread;
            this.commandService = commandService;
        }

        public ContentView CurrentView => GetCurrentContentView(Application.Current);

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            if (Application.Current is ShellWindow shellWindow)
                commandService.Value.RunAsync(keyEvent.KeyValue, GetContext(shellWindow.Content));

            return base.ProcessHotKey(keyEvent);
        }

        string GetContext(ContentView view) =>
            view != null ? contexts.GetOrAdd(view, x => x.GetType().GetCustomAttribute<ContentViewAttribute>()?.Context) : default;

        ContentView GetCurrentContentView(Toplevel view) =>
            view is ShellWindow shellWindow ? shellWindow.Content :
                view != null ? GetCurrentContentView(view.SuperView as Toplevel) : null;

        // Run the main window as soon as the app is presented.
        public override void WillPresent() => RunAsync(defaultView.Value);

        public Task RunAsync(ContentView view)
        {
            mainThread.Invoke(() =>
            {
                if (Application.Current is ShellWindow shellWindow)
                    shellWindow.Running = false;

                view.Refresh();

                Application.Run(shellWindows.GetOrAdd(view, x =>
                    new ShellWindow(view.Title, view, GetContext(view), commandService.Value)));
            });

            return Task.CompletedTask;
        }

        public async Task RunNext()
        {
            var viewList = views.ToList();

            var currentViewWithMetadata = viewList.FirstOrDefault(x => x.Metadata.Context == GetContext(CurrentView));

            var targetIndex = viewList.IndexOf(currentViewWithMetadata) + 1;
            if (targetIndex >= viewList.Count)
                targetIndex = 0;

            await RunAsync(viewList[targetIndex].Value);
        }

        public async Task RunPrevious()
        {
            var viewList = views.ToList();

            var currentViewWithMetadata = viewList.FirstOrDefault(x => x.Metadata.Context == GetContext(CurrentView));

            var targetIndex = viewList.IndexOf(currentViewWithMetadata) - 1;
            if (targetIndex < 0)
                targetIndex = viewList.Count - 1;

            await RunAsync(viewList[targetIndex].Value);
        }
    }
}
