using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Log
{
    [Shared]
    [MenuCommand("Reset", Key.F6, nameof(Log))]
    public class ResetCommand : IMenuCommand
    {
        readonly MainThread mainThread;
        readonly Repository repository;
        readonly LogView log;

        [ImportingConstructor]
        public ResetCommand(MainThread mainThread, Repository repository, LogView log)
        {
            this.mainThread = mainThread;
            this.repository = repository;
            this.log = log;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            var selectedCommit = log.SelectedCommit;

            if (selectedCommit != null)
            {
                var dialog = new ResetDialog();

                var result = mainThread.Invoke(() => dialog.ShowDialog());

                if (result == true)
                {
                    repository.Reset(dialog.ResetMode, log.SelectedCommit);

                    log.Refresh();
                }
            }

            return Task.CompletedTask;
        }
    }
}