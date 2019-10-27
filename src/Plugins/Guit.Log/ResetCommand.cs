using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Plugin.Log.Properties;
using LibGit2Sharp;

namespace Guit.Plugin.Log
{
    [Shared]
    [MenuCommand(WellKnownCommands.Log.Reset, 'r', WellKnownViews.Log, typeof(Resources))]
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

        public Task ExecuteAsync(CancellationToken cancellation = default)
        {
            if (view.SelectedEntry is CommitEntry selectedEntry)
            {
                var dialog = new ResetDialog(string.Format("Reset current branch {0} to {1}?", selectedEntry.Repository.Head.GetName(), selectedEntry.Commit.GetShortSha()));
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