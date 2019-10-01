using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Terminal.Gui;
using Git = LibGit2Sharp.Commands;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand("Revert", 'r', nameof(Changes))]
    public class RevertCommand : IMenuCommand
    {
        readonly MainThread mainThread;
        readonly Repository repository;
        readonly ChangesView changes;

        [ImportingConstructor]
        public RevertCommand(MainThread mainThread, Repository repository, ChangesView changes)
        {
            this.mainThread = mainThread;
            this.repository = repository;
            this.changes = changes;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            var entries = changes.GetMarkedEntries().ToList();

            if (entries.Any())
            {
                var dialog = new MessageBox("Revert Changes", string.Format("Are you sure you want to revert {0} item(s)?", entries.Count));
                var dialogResult = mainThread.Invoke(() => dialog.ShowDialog());

                if (dialogResult == true)
                {
                    foreach (var entry in entries)
                    {
                        switch (entry.State)
                        {
                            case FileStatus.ModifiedInWorkdir: repository.RevertFileChanges(entry.FilePath); break;
                            case FileStatus.NewInWorkdir: Git.Remove(repository, entry.FilePath); break;
                        }
                    }

                    changes.Refresh();
                }
            }

            return Task.CompletedTask;
        }
    }
}