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
    internal class App : Toplevel, IApp
    {
        Window main;
        readonly CommandService commandService;

        [ImportingConstructor]
        public App(
            // Force a RepositoryNotFoundException up-front.
            Repository repository,
            [ImportMany] IEnumerable<MainView> views,
            // Force all singletons to be instantiated.
            [ImportMany] IEnumerable<ISingleton> singletons,
            CommandService commandsService)
        {
            // Show an error window if we did not get at least one MainView.
            main = views.FirstOrDefault() ?? new Window("No MainView found!")
            {
                ColorScheme = Colors.Error,
            };

            this.commandService = commandsService;
        }

        // Run the main window as soon as the app is presented.
        public override void WillPresent() => RunAsync(main);

        Task IApp.RunAsync(MainView view, CancellationToken cancellation) => RunAsync(view, cancellation);

        private Task RunAsync(Window view, CancellationToken cancellation = default)
        {
            main.Running = false;
            main = view;

            // Check if the view is a MainView and if the CommandsView was not already set
            if (main is MainView mainView && mainView != null && mainView.CommandsView == null)
                mainView.CommandsView = commandService.GetCommandsView(mainView);

            return Task.Run(() => Application.MainLoop.Invoke(() => Application.Run(view)), cancellation);
        }

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            commandService.RunAsync(keyEvent.KeyValue, (main as MainView)?.Context);

            return base.ProcessHotKey(keyEvent);
        }
    }
}