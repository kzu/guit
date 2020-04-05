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
    [ChangesCommand(WellKnownCommands.Changes.Commit, 'c')]
    class CommitCommand : IMenuCommand
    {
        readonly IEventStream eventStream;
        readonly MainThread mainThread;
        readonly IGitRepository repository;
        readonly IChangesView view;

        [ImportingConstructor]
        public CommitCommand(IEventStream eventStream, MainThread mainThread, IGitRepository repository, IChangesView view)
        {
            this.eventStream = eventStream;
            this.mainThread = mainThread;
            this.repository = repository;
            this.view = view;
        }

        protected virtual bool CanExecute() => view.GetMarkedEntries().Any();

        protected virtual CommitOptions CreateCommitOptions() => new CommitOptions();

        protected virtual CommitDialog CreateDialog() => new CommitDialog("Commit");

        public Task ExecuteAsync(CancellationToken cancellation = default)
        {
            if (CanExecute())
            {
                CommitSubmodules();
                CommitChanges(repository, view.GetMarkedEntries(), CreateDialog());

                mainThread.Invoke(() => view.Refresh());
            }

            return Task.CompletedTask;
        }

        void CommitSubmodules()
        {
            foreach (var submoduleEntry in view.GetMarkedEntries(true))
            {
                if (repository.Submodules.FirstOrDefault(x => x.Path == submoduleEntry.FilePath) is Submodule submodule)
                {
                    using (var subRepo = new GitRepository(Path.Combine(repository.Info.WorkingDirectory, submodule.Path)))
                    {
                        var status = subRepo.RetrieveStatus();

                        var entries = status.Added
                            .Concat(status.Removed)
                            .Concat(status.Modified)
                            .Concat(status.Untracked)
                            .Concat(status.Missing);

                        if (entries.Any())
                            CommitChanges(subRepo, entries, new CommitDialog(string.Format("Commit Submodule {0}", submodule.Name)));
                    }
                }
            }
        }

        void CommitChanges(IGitRepository repository, IEnumerable<StatusEntry> entries, CommitDialog dialog)
        {
            if (mainThread.ShowDialog(dialog) == true)
            {
                if (!string.IsNullOrEmpty(dialog.NewBranchName))
                    repository.Checkout(repository.CreateBranch(dialog.NewBranchName));

                foreach (var entry in entries)
                    repository.Stage(entry.FilePath);

                eventStream.Push<Status>(0.5f);

                var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

                repository.Commit(
                    dialog.Message,
                    signature,
                    signature,
                    CreateCommitOptions());
            }
        }
    }
}