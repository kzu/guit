using System;
using System.Linq;
using System.Composition;
using LibGit2Sharp;
using Merq;

namespace Guit.Plugin.Changes
{
    [Shared]
    [ChangesCommand(WellKnownCommands.Changes.Amend, 'a')]
    class AmendCommitCommand : CommitCommand
    {
        readonly IGitRepository repository;

        [ImportingConstructor]
        public AmendCommitCommand(IEventStream eventStream, MainThread mainThread, IGitRepository repository, IChangesView changes)
            : base(eventStream, mainThread, repository, changes)
        {
            this.repository = repository;
        }

        Commit CommitToAmend => repository.Commits.FirstOrDefault();

        protected override bool CanExecute() => CommitToAmend != null;

        protected override CommitOptions CreateCommitOptions()
        {
            var commitOptions = base.CreateCommitOptions();
            commitOptions.AmendPreviousCommit = true;

            return commitOptions;
        }

        protected override CommitDialog CreateDialog()
        {
            var dialog = base.CreateDialog();
            dialog.Title = "Amend Commit";
            dialog.Message = CommitToAmend.Message;

            return dialog;
        }
    }
}