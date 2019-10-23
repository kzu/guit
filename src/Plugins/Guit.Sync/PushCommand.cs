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
    [SyncCommand(WellKnownCommands.Sync.Push, 'u')]
    public class PushCommand : IMenuCommand
    {
        readonly IEventStream eventStream;
        readonly MainThread mainThread;
        readonly Repository repository;
        readonly CredentialsHandler credentialsProvider;
        readonly SyncView view;

        [ImportingConstructor]
        public PushCommand(IEventStream eventStream, MainThread mainThread, Repository repository, CredentialsHandler credentialsProvider, SyncView view)
        {
            this.eventStream = eventStream;
            this.mainThread = mainThread;
            this.repository = repository;
            this.credentialsProvider = credentialsProvider;
            this.view = view;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var localBranch = repository.Head;
            var targetBranch = repository.Head.TrackedBranch ?? repository.Head;

            var dialog = new PushDialog(
                targetBranch.RemoteName ?? repository.GetDefaultRemoteName(),
                targetBranch.GetName(),
                trackRemoteBranch: repository.Head.TrackedBranch != null,
                remotes: repository.GetRemoteNames(),
                branches: repository.GetBranchNames());

            var result = mainThread.Invoke(() => dialog.ShowDialog());

            if (result == true && !string.IsNullOrEmpty(dialog.Remote))
            {
                var remote = repository
                    .Network
                    .Remotes
                    .FirstOrDefault(x => x.Name == dialog.Remote);

                if (remote != null)
                {
                    var pushRefSpec = $"refs/heads/{localBranch.GetName()}:refs/heads/{dialog.Branch}";
                    if (dialog.Force)
                        pushRefSpec = "+" + pushRefSpec;

                    var pushOptions = new PushOptions()
                    {
                        CredentialsProvider = credentialsProvider,
                        OnPushTransferProgress = (current, total, bytes) =>
                        {
                            eventStream.Push<Status>((float)current / (float)total);
                            return true;
                        }
                    };

                    eventStream.Push(Status.Start("git push {0} {1}", dialog.Remote, pushRefSpec));

                    repository.Network.Push(remote, pushRefSpec, pushOptions);

                    if (dialog.TrackRemoteBranch)
                    {
                        var trackedBranch = repository.Branches.FirstOrDefault(x =>
                            x.RemoteName == dialog.Remote && x.GetName() == dialog.Branch);

                        if (trackedBranch != null)
                            localBranch.Track(repository, trackedBranch);
                    }

                    eventStream.Push(Status.Succeeded());

                    mainThread.Invoke(() => view.Refresh());
                }
            }

            return Task.CompletedTask;
        }
    }
}