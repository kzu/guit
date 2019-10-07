using System;
using System.Composition;
using Terminal.Gui;

namespace Guit
{
    [Shared]
    class CommandExports
    {
        readonly Lazy<IShell> app;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public CommandExports(Lazy<IShell> app, MainThread mainThread)
        {
            this.app = app;
            this.mainThread = mainThread;
        }

        [MenuCommand("RunNext", Key.CursorRight, Visible = false, ReportProgress = false)]
        IMenuCommand RunNextCommand => new MenuCommand(async () => await app.Value.RunNext());

        [MenuCommand("RunPrevious", Key.CursorLeft, Visible = false, ReportProgress = false)]
        IMenuCommand RunPreviousCommand => new MenuCommand(async () => await app.Value.RunPrevious());

        [MenuCommand("Refresh", Key.F5, Visible = false, ReportProgress = false)]
        IMenuCommand RefreshCommand => new MenuCommand(() => mainThread.Invoke(() => (Application.Current as IRefreshPattern)?.Refresh()));

        [MenuCommand("SelectAll", '*', Visible = false, ReportProgress = false)]
        IMenuCommand SelectAllCommand => new MenuCommand(() => mainThread.Invoke(() => app.Value.CurrentView?.SelectAll(invertSelection: true)));
    }
}