using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Guit.Sync.Properties;
using LibGit2Sharp;
using Merq;

namespace Guit.Plugin.Sync
{
    [Shared]
    [MenuCommand("Sync.Pull", 'p', nameof(Sync), typeof(Resources))]
    public class PullCommand : IMenuCommand
    {
        readonly MainThread mainThread;
        readonly IRepository repository;
        readonly IEventStream eventStream;

        [ImportingConstructor]
        public PullCommand(MainThread mainThread, IRepository repository, IEventStream eventStream)
        {
            this.mainThread = mainThread;
            this.repository = repository;
            this.eventStream = eventStream;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            var localBranch = repository.Head;

            var targetBranch = repository.Head.TrackedBranch ?? repository.Head;

            var dialog = new PullDialog(
                targetBranch.RemoteName ?? repository.GetDefaultRemoteName(),
                targetBranch.GetName(),
                trackRemoteBranch: false,
                remotes: repository.GetRemoteNames(),
                branches: repository.GetBranchNames());

            var result = mainThread.Invoke(() => dialog.ShowDialog());
            if (result == true && !string.IsNullOrEmpty(dialog.Branch))
            {
                var targetBranchFriendlyName = string.IsNullOrEmpty(dialog.Remote) ?
                    dialog.Branch : $"{dialog.Remote}/{dialog.Branch}";

                targetBranch = repository.Branches
                    .FirstOrDefault(x => x.FriendlyName == targetBranchFriendlyName);

                if (targetBranch == null)
                    throw new InvalidOperationException(string.Format("Branch {0} not found", targetBranchFriendlyName));

                eventStream.Push(Status.Start("Pull {0} {1}", targetBranchFriendlyName, dialog.IsFastForward ? "With Fast Fordward" : string.Empty));

                // 1. Merge
                var mergeResult = repository.Merge(
                    targetBranch,
                    repository.Config.BuildSignature(DateTimeOffset.Now),
                    new MergeOptions()
                    {
                        FastForwardStrategy = dialog.IsFastForward ?
                            FastForwardStrategy.FastForwardOnly : FastForwardStrategy.NoFastForward
                    });

                // 2. Track
                if (dialog.TrackRemoteBranch)
                    localBranch.Track(repository, targetBranch);

                // 3. Update submodules
                if (dialog.UpdateSubmodules)
                {
                    eventStream.Push(Status.Create(0.8f, "Updating submodules..."));
                    repository.UpdateSubmodules(eventStream: eventStream);
                }

                eventStream.Push(Status.Finish(mergeResult.Status.ToString()));
            }

            return Task.CompletedTask;
        }
    }
}