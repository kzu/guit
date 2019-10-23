using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Guit.Plugin.Sync.Properties;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Merq;

namespace Guit.Plugin.Sync
{
    [Shared]
    [SyncCommand(WellKnownCommands.Sync.Pull, 'p')]
    public class PullCommand : IMenuCommand
    {
        readonly MainThread mainThread;
        readonly IRepository repository;
        readonly IEventStream eventStream;
        readonly CredentialsHandler credentials;
        readonly ICommandService commandService;
        readonly IShell shell;
        readonly SyncView view;

        [ImportingConstructor]
        public PullCommand(
            MainThread mainThread,
            IRepository repository,
            IEventStream eventStream,
            CredentialsHandler credentials,
            ICommandService commandService,
            IShell shell,
            SyncView view)
        {
            this.mainThread = mainThread;
            this.repository = repository;
            this.eventStream = eventStream;
            this.credentials = credentials;
            this.commandService = commandService;
            this.shell = shell;
            this.view = view;
        }

        public async Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var repositoryStatus = repository.RetrieveStatus();

            var localBranch = repository.Head;
            var targetBranch = repository.Head.TrackedBranch ?? repository.Head;

            var dialog = new PullDialog(
                targetBranch.RemoteName ?? repository.GetDefaultRemoteName(),
                targetBranch.GetName(),
                showStashWarning: repositoryStatus.IsDirty,
                trackRemoteBranch: false,
                remotes: repository.GetRemoteNames(),
                branches: repository.GetBranchNames());

            if (mainThread.Invoke(() => dialog.ShowDialog()) == true && !string.IsNullOrEmpty(dialog.Branch))
            {
                var targetBranchFriendlyName = string.IsNullOrEmpty(dialog.Remote) ?
                    dialog.Branch : $"{dialog.Remote}/{dialog.Branch}";

                targetBranch = repository.Branches.FirstOrDefault(x => x.FriendlyName == targetBranchFriendlyName);

                if (targetBranch == null)
                    throw new InvalidOperationException(string.Format("Branch {0} not found", targetBranchFriendlyName));

                eventStream.Push(Status.Start("Pull {0} {1}", targetBranchFriendlyName, dialog.IsFastForward ? "With Fast Fordward" : string.Empty));

                var stash = default(Stash);
                var mergeResult = default(MergeResult);
                var stashResult = default(StashApplyStatus);

                // 1. Fetch
                if (targetBranch.IsRemote)
                    TryFetch(targetBranch);

                // 2. Stash (optional, if the repo is dirty) and Merge
                try
                {
                    if (repositoryStatus.IsDirty)
                        stash = repository.Stashes.Add(Signatures.GetStashSignature(), StashModifiers.IncludeUntracked);

                    mergeResult = Merge(targetBranch, dialog.IsFastForward);
                }
                finally
                {
                    if (stash != null && repository.Stashes.Contains(stash) && !repository.RetrieveStatus().IsDirty)
                        stashResult = repository.Stashes.Pop(repository.Stashes.ToList().IndexOf(stash));
                }

                // 3. Resolve conflicts
                if (mergeResult?.Status == MergeStatus.Conflicts)
                    await commandService.RunAsync("ResolveConflicts");

                // 4. Track
                if (dialog.TrackRemoteBranch)
                    localBranch.Track(repository, targetBranch);

                // 5. Update submodules
                if (dialog.UpdateSubmodules)
                {
                    eventStream.Push(Status.Create(0.8f, "Updating submodules..."));
                    repository.UpdateSubmodules(eventStream: eventStream);
                }

                eventStream.Push(Status.Finish(mergeResult.Status.ToString()));

                mainThread.Invoke(() => view.Refresh());
            }
        }

        void TryFetch(Branch targetBranch)
        {
            try
            {
                repository.Fetch(targetBranch.RemoteName, credentials);
            }
            catch (Exception ex)
            {
                eventStream.Push(Status.Create("Unable to fetch from remote '{0}': {1}", targetBranch.RemoteName, ex.Message));
            }
        }

        MergeResult Merge(Branch targetBranch, bool fastForward) =>
            repository.Merge(
                targetBranch,
                repository.Config.BuildSignature(DateTimeOffset.Now),
                new MergeOptions()
                {
                    CommitOnSuccess = true,
                    FastForwardStrategy = fastForward ?
                        FastForwardStrategy.FastForwardOnly : FastForwardStrategy.NoFastForward
                });
    }
}