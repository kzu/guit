using System;
using System.Composition;
using Terminal.Gui;

namespace Guit
{
    [Shared]
    class CommandExports
    {
        readonly Lazy<IApp> app;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public CommandExports(Lazy<IApp> app, MainThread mainThread)
        {
            this.app = app;
            this.mainThread = mainThread;
        }

        [MenuCommand("RunNext", Key.CursorRight, Visible = false)]
        IMenuCommand RunNextCommand => new MenuCommand(async () => await app.Value.RunNext());

        [MenuCommand("RunPrevious", Key.CursorLeft, Visible = false)]
        IMenuCommand RunPreviousCommand => new MenuCommand(async () => await app.Value.RunPrevious());

        [MenuCommand("Refresh", Key.F5, Visible = false)]
        IMenuCommand RefreshCommand => new MenuCommand(() => mainThread.Invoke(() => app.Value.Current?.Refresh()));

        [MenuCommand("SelectAll", '*', Visible = false)]
        IMenuCommand SelectAllCommand => new MenuCommand(() => mainThread.Invoke(() => app.Value.Current?.SelectAll(invertSelection: true)));
    }
}