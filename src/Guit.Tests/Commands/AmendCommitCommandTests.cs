using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Guit.Plugin.Changes;
using LibGit2Sharp;
using Merq;
using Moq;
using Xunit;

namespace Guit.Tests.Commands
{
    public class AmendCommitCommandTests
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

        public AmendCommitCommandTests()
        {
            eventStream = Mock.Of<IEventStream>();
            repository = Mock.Of<IGitRepository>(x => x.Config == Mock.Of<LibGit2Sharp.Configuration>());
            mainThread = Mock.Of<MainThread>();
            view = Mock.Of<IChangesView>();
        }

        IMenuCommand CreateCommand() =>
            new AmendCommitCommand(eventStream, mainThread, repository, view);

        [Fact]
        public async Task when_there_is_no_changes_and_no_commits_then_amend_commit_is_not_executed()
        {
            var command = CreateCommand();

            Mock.Get(view)
                .Setup(x => x.GetMarkedEntries(It.IsAny<bool?>()))
                .Returns(Enumerable.Empty<StatusEntry>());

            Mock.Get(repository)
                .Setup(x => x.Commits)
                .Returns(new QueryableCommitLog());

            await command.ExecuteAsync();

            Mock.Get(repository).Verify(
                x => x.Commit(It.IsAny<string>(), It.IsAny<Signature>(), It.IsAny<Signature>(), It.IsAny<CommitOptions>()),
                Times.Never);
        }

        [Fact]
        public async Task when_amending_commit_then_changes_are_staged_and_committed()
        {
            var command = CreateCommand();

            Mock.Get(mainThread)
                .Setup(x => x.ShowDialog(It.IsAny<CommitDialog>()))
                .Returns(true);

            Mock.Get(view)
                .Setup(x => x.GetMarkedEntries(default))
                .Returns(Changes);

            var signature = new Signature("user", "user@guit.com", DateTimeOffset.Now);
            Mock.Get(repository.Config)
                .Setup(x => x.BuildSignature(It.IsAny<DateTimeOffset>()))
                .Returns(signature);

            var commitToBeAmended = Mock.Of<Commit>(x => x.Message == "Committing foo");
            Mock.Get(repository)
                .Setup(x => x.Commits)
                .Returns(new QueryableCommitLog(commitToBeAmended));

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
                    It.Is<CommitOptions>(options => options.AmendPreviousCommit)));
        }

        class QueryableCommitLog : List<Commit>, IQueryableCommitLog
        {
            public QueryableCommitLog()
            { }

            public QueryableCommitLog(params Commit[] commits)
                : base(commits)
            { }

            public CommitSortStrategies SortedBy => throw new NotImplementedException();

            public ICommitLog QueryBy(CommitFilter filter) =>
                throw new NotImplementedException();

            public IEnumerable<LogEntry> QueryBy(string path) =>
                throw new NotImplementedException();

            public IEnumerable<LogEntry> QueryBy(string path, CommitFilter filter) =>
                throw new NotImplementedException();
        }
    }
}