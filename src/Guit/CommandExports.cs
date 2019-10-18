using System;
using System.Linq;
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

        [MenuCommand("Filter", Key.F6, Visible = false, ReportProgress = false)]
        IMenuCommand FilterCommand => new MenuCommand(() => mainThread.Invoke(() =>
        {
            if (Application.Current is IFilterPattern filterPattern)
            {
                var dialog = new InputBox("Filter", "Enter values seperated by ',' or leave empty");
                dialog.Text = filterPattern.Filter is null ? string.Empty : string.Join(", ", filterPattern.Filter);

                if (dialog.ShowDialog() == true)
                {
                    filterPattern.Filter = string.IsNullOrEmpty(dialog.Text) ? default :
                        dialog.Text
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim())
                            .Where(x => !string.IsNullOrEmpty(x))
                            .ToArray();
                }
            }
        }));
    }
}