using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
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
    [CherryPickerCommand(WellKnownCommands.CherryPicker.Push, 'u', IsDynamic = true)]
    class PushCommand : IDynamicMenuCommand
    {
        readonly IEventStream eventStream;
        readonly MainThread mainThread;
        readonly CherryPickerView view;
        readonly CredentialsHandler credentials;
        readonly IEnumerable<CherryPickConfig> repositories;

        [ImportingConstructor]
        public PushCommand(
            IEventStream eventStream,
            MainThread mainThread,
            CherryPickerView view,
            CredentialsHandler credentials,
            IEnumerable<CherryPickConfig> repositories)
        {
            this.eventStream = eventStream;
            this.mainThread = mainThread;
            this.view = view;
            this.credentials = credentials;
            this.repositories = repositories;

            IsVisible = IsEnabled = !view.IsRootMode;
        }

        public bool IsVisible { get; }

        public bool IsEnabled { get; }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var dirtyRepositories = repositories
                .Select(config => (
                    config,
                    targetBranch: config.Repository.GetBranch(config.TargetBranch),
                    targetBranchRemote: config.Repository.GetBranch(config.TargetBranchRemote)))
                .Where(x => x.targetBranch != null && x.targetBranch.Tip != x.targetBranchRemote?.Tip)
                .Select(x => (x.config.Repository.GetName(), x.config.TargetBranchRemote + $"-merge-{x.targetBranch.Tip.GetShortSha()}"));

            if (dirtyRepositories.Any())
            {
                var dialog = new PushDialog(dirtyRepositories.ToArray());

                if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
                {
                    foreach (var branch in dialog.Branches)
                    {
                        if (branch.BranchName?.Contains('/') == true && repositories.FirstOrDefault(x => x.Repository.GetName() == branch.Repo) is CherryPickConfig config)
                        {
                            var repository = config.Repository;
                            var remoteName = branch.BranchName.Substring(0, branch.BranchName.IndexOf('/'));

                            if (repository.Network.Remotes.FirstOrDefault(x => x.Name == remoteName) is Remote remote)
                            {
                                var targetBranchName = branch.BranchName.Substring(remoteName.Length + 1);

                                // Push
                                config.Repository.Network.Push(
                                    repository.Network.Remotes.Single(x => x.Name == "origin"),
                                    $"refs/heads/{repository.Head.FriendlyName}:refs/heads/{targetBranchName}",
                                    new PushOptions { CredentialsProvider = credentials });

                                eventStream.Push(Status.Create("Pushed changes to {0}", $"{repository.GetRepoUrl()}/tree/{targetBranchName}"));

                                Process.Start("cmd", $"/c start {repository.GetRepoUrl()}/compare/{config.TargetBranch}...{targetBranchName}");

                                mainThread.Invoke(() => view.Refresh());
                            }
                            else
                            {
                                mainThread.Invoke(() => new MessageBox("Error", "Remote '{0}' not found for '{1}'", remoteName, branch.BranchName).ShowDialog());
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}