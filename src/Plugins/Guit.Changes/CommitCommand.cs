using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using Merq;

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

        protected bool Amend { get; set; }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            if (changes.GetMarkedEntries().Any())
            {
                var dialog = new CommitDialog();

                if (Amend)
                    dialog.Message = repository.Commits.FirstOrDefault()?.Message;

                var dialogResult = mainThread.Invoke(() => dialog.ShowDialog());

                if (dialogResult == true)
                {
                    eventStream.Push<Status>(0.5f);

                    if (!string.IsNullOrEmpty(dialog.NewBranchName))
                        repository.Checkout(repository.CreateBranch(dialog.NewBranchName));

                    foreach (var entry in changes.GetMarkedEntries())
                        repository.Stage(entry.FilePath);

                    var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

                    var options = new CommitOptions
                    {
                        AmendPreviousCommit = Amend
                    };

                    repository.Commit(dialog.Message, signature, signature, options);

                    mainThread.Invoke(() => changes.Refresh());
                }
            }

            return Task.CompletedTask;
        }
    }
}