using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit
{
    [Export]
    [Export(typeof(IApp))]
    [Shared]
    class App : Toplevel, IApp
    {
        readonly ConcurrentDictionary<ContentView, string> contexts = new ConcurrentDictionary<ContentView, string>();
        readonly Lazy<ContentView, MenuCommandMetadata> defaultView;
        readonly MainThread mainThread;
        readonly Lazy<CommandService> commandService;

        [ImportingConstructor]
        public App(
            [ImportMany] IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views,
            MainThread mainThread,
            Lazy<CommandService> commandService)
        {
            this.defaultView = views
                .OrderBy(x => x.Metadata.Order)
                .ThenBy(x => x.Metadata.Key)
                .First();

            this.mainThread = mainThread;
            this.commandService = commandService;
        }

        public ContentView Current => GetCurrentContentView(Application.Current);

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            commandService.Value.RunAsync(keyEvent.KeyValue, GetContext(Application.Current as ContentView));

            return base.ProcessHotKey(keyEvent);
        }

        string GetContext(ContentView view) =>
            view != null ?
                contexts.GetOrAdd(view, x =>
                {
                    var contentViewAttribute = x.GetType().GetCustomAttributes(typeof(ContentViewAttribute), false).FirstOrDefault() as ContentViewAttribute;

                    return contentViewAttribute?.Context;
                }) : default;

        ContentView GetCurrentContentView(Toplevel view) =>
            view is ContentView contentView && contentView != null ? contentView :
                view != null ? GetCurrentContentView(view.SuperView as Toplevel) : null;

        // Run the main window as soon as the app is presented.
        public override void WillPresent() => RunAsync(defaultView.Value);

        public Task RunAsync(ContentView view)
        {
            mainThread.Invoke(() =>
            {
                var currentView = Current;
                if (currentView != null)
                    currentView.Running = false;

                if (view.Commands == null)
                    view.Commands = commandService.Value.GetCommands(view, GetContext(view));

                view.Refresh();

                Application.Run(view);
            });

            return Task.CompletedTask;
        }
    }
}
