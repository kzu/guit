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
            var targetBranchName = targetBranch.GetName();

            var dialog = new PullDialog(
                targetBranch.RemoteName ?? repository.GetDefaultRemoteName(),
                targetBranchName,
                trackRemoteBranch: repository.Head.TrackedBranch != null,
                remotes: repository.GetRemoteNames(),
                branches: repository.GetBranchNames());

            var result = mainThread.Invoke(() => dialog.ShowDialog());
            if (result == true && !string.IsNullOrEmpty(dialog.Remote) && !string.IsNullOrEmpty(dialog.Branch))
            {
                targetBranch = repository
                    .Branches
                    .FirstOrDefault(x =>
                        x.RemoteName == dialog.Remote &&
                        x.CanonicalName.EndsWith(dialog.Branch));

                if (targetBranch == null)
                    throw new InvalidOperationException(string.Format("Branch {0}:{1} not found", dialog.Remote, dialog.Branch));

                eventStream.Push(Status.Start("Pull {0} {1} {2}", targetBranch.RemoteName, targetBranch.FriendlyName, dialog.IsFastForward ? "--ff-only" : string.Empty));

                var mergeResult = repository.Merge(
                    targetBranch,
                    repository.Config.BuildSignature(DateTimeOffset.Now),
                    new MergeOptions()
                    {
                        FastForwardStrategy = dialog.IsFastForward ?
                            FastForwardStrategy.FastForwardOnly : FastForwardStrategy.NoFastForward
                    });

                if (dialog.TrackRemoteBranch)
                    localBranch.Track(repository, targetBranch);

                eventStream.Push(Status.Finish(mergeResult.Status.ToString()));
            }

            return Task.CompletedTask;
        }
    }
}