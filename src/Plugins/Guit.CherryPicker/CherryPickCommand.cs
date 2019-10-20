using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Merq;

namespace Guit.Plugin.CherryPicker
{
    [Shared]
    [MenuCommand("Cherry-Pick", 'c', nameof(CherryPicker))]
    class ConsolidateCommand : IMenuCommand, IAfterExecuteCallback
    {
        readonly IEventStream eventStream;
        readonly MainThread mainThread;
        readonly CherryPickerView view;
        readonly CredentialsHandler credentials;

        [ImportingConstructor]
        public ConsolidateCommand(IEventStream eventStream, MainThread mainThread, CherryPickerView view, CredentialsHandler credentials)
        {
            this.eventStream = eventStream;
            this.mainThread = mainThread;
            this.view = view;
            this.credentials = credentials;
        }

        public Task AfterExecuteAsync(CancellationToken cancellation)
        {
            mainThread.Invoke(() => view.Refresh());

            return Task.CompletedTask;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            foreach (var repositoryEntries in view.MarkedEntries.GroupBy(x => x.Config).Where(x => x.Any()))
            {
                var config = repositoryEntries.Key;
                var repository = config.Repository;

                var dialog = new MessageBox(
                    string.Format("{0} ({1} -> {2})", config.Repository.GetName(), config.BaseBranch, config.TargetBranch),
                    DialogBoxButton.Ok | DialogBoxButton.Cancel,
                    string.Format("We will cherry pick the selected {0} commit(s) into the following branch: {1}", repositoryEntries.Count(), config.TargetBranch));

                if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
                {
                    var targetBranch = repository.SwitchToTargetBranch(config);

                    var count = 0;
                    foreach (var entry in repositoryEntries.Reverse())
                    {
                        var result = repository.CherryPick(entry.Commit, repository.Config.BuildSignature(DateTimeOffset.Now));

                        if (result.Status == CherryPickStatus.Conflicts)
                        {
                            repository.Reset(ResetMode.Hard);

                            throw new InvalidOperationException(string.Format("Unable to cherry pick {0}", entry.Commit.MessageShort));
                        }
                        else
                        {
                            eventStream.Push(Status.Create(++count / (float)repositoryEntries.Count(), "Cherry picking {0} {1}", entry.Commit.GetShortSha(), entry.Commit.MessageShort));
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}