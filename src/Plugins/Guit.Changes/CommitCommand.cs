using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using Merq;
using Git = LibGit2Sharp.Commands;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand("Commit", 'c', nameof(Changes))]
    public class CommitCommand : IMenuCommand
    {
        readonly IEventStream eventStream;
        readonly MainThread mainThread;
        readonly IRepository repository;
        readonly ChangesView changes;

        [ImportingConstructor]
        public CommitCommand(IEventStream eventStream, MainThread mainThread, IRepository repository, ChangesView changes)
        {
            this.eventStream = eventStream;
            this.mainThread = mainThread;
            this.repository = repository;
            this.changes = changes;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            if (changes.GetMarkedEntries().Any())
            {
                var dialog = new CommitDialog();
                var dialogResult = mainThread.Invoke(() => dialog.ShowDialog());

                if (dialogResult == true)
                {
                    eventStream.Push<Status>(0.5f);

                    if (!string.IsNullOrEmpty(dialog.NewBranchName))
                        Git.Checkout(repository, repository.CreateBranch(dialog.NewBranchName));

                    foreach (var entry in changes.GetMarkedEntries())
                        Git.Stage(repository, entry.FilePath);

                    var signature = repository.Config.BuildSignature(DateTimeOffset.Now);
                    repository.Commit(dialog.Message, signature, signature);

                    mainThread.Invoke(() => changes.Refresh());
                }
            }

            return Task.CompletedTask;
        }
    }
}