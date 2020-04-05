using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Guit.Plugin.Changes;
using LibGit2Sharp;
using Merq;
using Moq;
using Xunit;

namespace Guit.Tests.Commands
{
    public class CommitCommandTests
    {
        public static StatusEntry[] Changes = new[]
        {
            Mock.Of<StatusEntry>(x => x.FilePath == "src\\Foo.cs"),
            Mock.Of<StatusEntry>(x => x.FilePath == "src\\Folder\\Boo.cs"),
        };

        IEventStream eventStream;
        IGitRepository repository;
        MainThread mainThread;
        IChangesView view;

        public CommitCommandTests()
        {
            eventStream = Mock.Of<IEventStream>();
            repository = Mock.Of<IGitRepository>(x => x.Config == Mock.Of<LibGit2Sharp.Configuration>());
            mainThread = Mock.Of<MainThread>();
            view = Mock.Of<IChangesView>();
        }

        IMenuCommand CreateCommand() =>
            new CommitCommand(eventStream, mainThread, repository, view);

        [Fact]
        public async Task when_there_is_no_changes_then_commit_is_not_executed()
        {
            var command = CreateCommand();

            Mock.Get(view)
                .Setup(x => x.GetMarkedEntries(It.IsAny<bool?>()))
                .Returns(Enumerable.Empty<StatusEntry>());

            await command.ExecuteAsync();

            Mock.Get(repository).Verify(
                x => x.Commit(It.IsAny<string>(), It.IsAny<Signature>(), It.IsAny<Signature>(), It.IsAny<CommitOptions>()),
                Times.Never);
        }

        [Fact]
        public async Task when_committing_changes_then_changes_are_staged_and_committed()
        {
            var command = CreateCommand();

            Mock.Get(mainThread)
                .Setup(x => x.ShowDialog(It.IsAny<CommitDialog>()))
                .Callback<CommitDialog>(dialog => dialog.Message = "Committing foo")
                .Returns(true);

            Mock.Get(view)
                .Setup(x => x.GetMarkedEntries(default))
                .Returns(Changes);

            var signature = new Signature("user", "user@guit.com", DateTimeOffset.Now);
            Mock.Get(repository.Config)
                .Setup(x => x.BuildSignature(It.IsAny<DateTimeOffset>()))
                .Returns(signature);

            await command.ExecuteAsync();

            // Verify if all changes have been staged
            foreach (var change in Changes)
                Mock.Get(repository).Verify(x => x.Stage(change.FilePath));

            // And commited
            Mock.Get(repository)
                .Verify(x => x.Commit(
                    "Committing foo",
                    signature,
                    signature,
                    It.IsAny<CommitOptions>()));
        }

        [Fact]
        public async Task when_committing_changes_and_new_branch_name_is_specified_then_branch_is_checked_out()
        {
            var command = CreateCommand();

            Mock.Get(mainThread)
                .Setup(x => x.ShowDialog(It.IsAny<CommitDialog>()))
                .Callback<CommitDialog>(dialog => dialog.NewBranchName = "feature/foo")
                .Returns(true);

            Mock.Get(view)
                .Setup(x => x.GetMarkedEntries(default))
                .Returns(Changes);

            var branch = Mock.Of<Branch>(x => x.FriendlyName == "feature/foo");
            Mock.Get(repository)
                .Setup(x => x.CreateBranch("feature/foo"))
                .Returns(branch);

            await command.ExecuteAsync();

            Mock.Get(repository).Verify(x => x.Checkout(branch));
        }
    }
}