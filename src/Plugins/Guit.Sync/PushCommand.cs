using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Guit.Sync.Properties;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Merq;

namespace Guit.Plugin.Sync
{
    [Shared]
    [MenuCommand("Sync.Push", 'u', nameof(Sync), typeof(Resources))]
    public class PushCommand : IMenuCommand
    {
        readonly IEventStream eventStream;
        readonly MainThread mainThread;
        readonly Repository repository;
        readonly CredentialsHandler credentialsProvider;

        [ImportingConstructor]
        public PushCommand(IEventStream eventStream, MainThread mainThread, Repository repository, CredentialsHandler credentialsProvider)
        {
            this.eventStream = eventStream;
            this.mainThread = mainThread;
            this.repository = repository;
            this.credentialsProvider = credentialsProvider;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            var localBranch = repository.Head;
            var targetBranch = repository.Head.TrackedBranch ?? repository.Head;
            var targetBranchName = targetBranch.GetName();

            var dialog = new PushDialog(
                targetBranch.RemoteName ?? repository.GetDefaultRemoteName(),
                targetBranchName,
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
                }
            }

            return Task.CompletedTask;
        }
    }
}