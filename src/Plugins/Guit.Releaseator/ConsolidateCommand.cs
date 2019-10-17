using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Merq;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    [MenuCommand("Consolidate", 'c', nameof(Releaseator))]
    class ConsolidateCommand : IMenuCommand, IAfterExecuteCallback
    {
        readonly IEventStream eventStream;
        readonly MainThread mainThread;
        readonly ReleaseatorView view;
        readonly CredentialsHandler credentials;

        [ImportingConstructor]
        public ConsolidateCommand(IEventStream eventStream, MainThread mainThread, ReleaseatorView view, CredentialsHandler credentials)
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
                    string.Format("{0} ({1} -> {2} -> {3})", config.Repository.GetName(), config.BaseBranch, config.MergeBranch, config.ReleaseBranch),
                    DialogBoxButton.Ok | DialogBoxButton.Cancel,
                    string.Format("We will cherry pick the selected {0} commit(s) into the following merge branch: {1}", repositoryEntries.Count(), config.MergeBranch));

                if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
                {
                    // Search the local merge branch
                    if (repository.SwitchToMergeBranch(config, true) is Branch mergeBranch)
                    {
                        // Cherrypick commits
                        foreach (var entry in repositoryEntries.Reverse())
                        {
                            var result = repository.CherryPick(entry.Commit, repository.Config.BuildSignature(DateTimeOffset.Now));

                            if (result.Status == CherryPickStatus.Conflicts)
                            {
                                repository.Reset(ResetMode.Hard);

                                throw new InvalidOperationException(string.Format("Unable to cherry pick {0}", entry.Commit.MessageShort));

                                //eventStream.Push(Status.Create("Failed cherry pick {0} {1}", entry.Commit.Sha, entry.Commit.MessageShort));
                            }
                            else
                            {
                                eventStream.Push(Status.Create("Cherry pick {0} {1}", entry.Commit.Sha, entry.Commit.MessageShort));
                            }
                        }

                        // Push
                        repository.Network.Push(
                            repository.Network.Remotes.Single(x => x.Name == "origin"),
                            $"refs/heads/{config.MergeBranch}:refs/heads/{config.MergeBranch}",
                            new PushOptions { CredentialsProvider = credentials });

                        eventStream.Push(Status.Create("Pushed changes to {0}", $"{repository.GetRepoUrl()}/tree/{config.MergeBranch}"));

                        Process.Start("cmd", $"/c start {repository.GetRepoUrl()}/compare/{config.ReleaseBranch}...{config.MergeBranch}");
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}