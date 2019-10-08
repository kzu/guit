using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
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
                foreach (var submoduleEntry in changes.GetMarkedEntries(true))
                {
                    var submodule = repository.Submodules.FirstOrDefault(x => x.Path == submoduleEntry.FilePath);
                    if (submodule != null)
                    {
                        using (var subRepo = new Repository(Path.Combine(repository.Info.WorkingDirectory, submodule.Path)))
                        {
                            var status = subRepo.RetrieveStatus();
                            var entries = status.Added
                                .Concat(status.Removed)
                                .Concat(status.Modified)
                                .Concat(status.Untracked)
                                .Concat(status.Missing);

                            Commit(subRepo, entries, string.Format("Commit Submodule {0}", submodule.Name));
                        }
                    }
                }

                Commit(repository, changes.GetMarkedEntries(), reportProgress: true);

                mainThread.Invoke(() => changes.Refresh());
            }

            return Task.CompletedTask;
        }

        void Commit(IRepository repository, IEnumerable<StatusEntry> entries, string title = "Commit", bool reportProgress = false)
        {
            if (entries.Any())
            {
                var dialog = new CommitDialog(title);

                if (Amend)
                    dialog.Message = repository.Commits.FirstOrDefault()?.Message;

                if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
                {
                    if (!string.IsNullOrEmpty(dialog.NewBranchName))
                        repository.Checkout(repository.CreateBranch(dialog.NewBranchName));

                    foreach (var entry in entries)
                        repository.Stage(entry.FilePath);

                    var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

                    var options = new CommitOptions
                    {
                        AmendPreviousCommit = Amend
                    };

                    eventStream.Push<Status>(0.5f);

                    repository.Commit(dialog.Message, signature, signature, options);
                }
            }
        }
    }
}