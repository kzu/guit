using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using Merq;
using Microsoft.VisualStudio.Threading;
using Terminal.Gui;
using Git = LibGit2Sharp.Commands;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand("Commit", Key.F6, nameof(Changes))]
    public class CommitCommand : IMenuCommand
    {
        readonly IEventStream eventStream;
        readonly JoinableTaskFactory jtf;
        readonly Repository repository;
        readonly ChangesView changes;

        [ImportingConstructor]
        public CommitCommand(IEventStream eventStream, JoinableTaskFactory jtf, Repository repository, ChangesView changes)
        {
            this.eventStream = eventStream;
            this.jtf = jtf;
            this.repository = repository;
            this.changes = changes;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            var dialog = new CommitDialog();

            var dialogResult = jtf.Run(async () =>
            {
                await jtf.SwitchToMainThreadAsync();

                return dialog.ShowDialog();
            });

            if (dialogResult == true)
            {
                if (!string.IsNullOrEmpty(dialog.NewBranchName))
                    Git.Checkout(repository, repository.CreateBranch(dialog.NewBranchName));

                foreach (var entry in changes.GetMarkedEntries())
                    Git.Stage(repository, entry.FilePath);

                var signatrure = repository.Config.BuildSignature(DateTimeOffset.Now);
                repository.Commit(dialog.Message, signatrure, signatrure);

                eventStream.Push<Status>("Commit OK!");
            }

            return Task.CompletedTask;
        }
    }
}