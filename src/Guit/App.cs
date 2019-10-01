using System;
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
        ContentView main;
        string context;

        readonly MainThread mainThread;
        readonly Lazy<CommandService> commandService;

        [ImportingConstructor]
        public App(
            [ImportMany] IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views,
            MainThread mainThread,
            Lazy<CommandService> commandService)
        {
            var defaultView = views.FirstOrDefault();

            main = defaultView?.Value;
            context = defaultView?.Metadata.Context;

            this.mainThread = mainThread;
            this.commandService = commandService;
        }

        public ContentView Current => main;

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            commandService.Value.RunAsync(keyEvent.KeyValue, context);

            return base.ProcessHotKey(keyEvent);
        }

        // Run the main window as soon as the app is presented.
        public override void WillPresent() => RunAsync(main, context);

        public Task RunAsync(ContentView view, string context = null)
        {
            main = view;
            this.context = context;
            mainThread.Invoke(() =>
            {
                main.Running = false;

                if (main.Commands == null)
                    main.Commands = commandService.Value.GetCommands(main, context);

                main.Refresh();

                Application.Run(main);
            });

            return Task.CompletedTask;
        }
    }
}
