using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Plugin;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit
{
    [Export]
    [Export(typeof(IApp))]
    [Shared]
    class App : Toplevel, IApp
    {
        Window main;
        readonly MainThread mainThread;
        readonly CommandService commandService;

        [ImportingConstructor]
        public App(
            [ImportMany] IEnumerable<ContentView> views,
            MainThread mainThread,
            CommandService commandService)
        {
            // Show an error window if we did not get at least one MainView.
            main = views.FirstOrDefault() ?? new Window("No MainView found!")
            {
                ColorScheme = Colors.Error,
            };

            this.mainThread = mainThread;
            this.commandService = commandService;
        }

        public ContentView Current => main as ContentView;

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            commandService.RunAsync(keyEvent.KeyValue, (main as ContentView)?.Context);

            return base.ProcessHotKey(keyEvent);
        }

        // Run the main window as soon as the app is presented.
        public override void WillPresent() => RunAsync(main);

        Task IApp.RunAsync(ContentView view) => RunAsync(view);

        Task RunAsync(Window view)
        {
            main.Running = false;
            main = view;

            // Check if the view is a MainView and if the CommandsView was not already set
            if (main is ContentView mainView && mainView != null)
            {
                if (mainView.Commands == null)
                    mainView.Commands = commandService.GetCommands(mainView);

                mainView.Refresh();
            }

            mainThread.Invoke(() => Application.Run(view));

            return Task.CompletedTask;
        }
    }
}