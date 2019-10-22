using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Guit.Plugin.Log
{
    [Shared]
    [MenuCommand("Reset", 'r', WellKnownViews.Log)]
    class ResetCommand : IMenuCommand
    {
        readonly MainThread mainThread;
        readonly IRepository repository;
        readonly LogView view;

        [ImportingConstructor]
        public ResetCommand(MainThread mainThread, IRepository repository, LogView view)
        {
            this.mainThread = mainThread;
            this.repository = repository;
            this.view = view;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            if (view.SelectedEntry is CommitEntry selectedEntry)
            {
                var dialog = new ResetDialog();
                if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
                {
                    repository.Reset(dialog.ResetMode, view.SelectedEntry.Commit);

                    view.Refresh();
                }
            }

            return Task.CompletedTask;
        }
    }
}