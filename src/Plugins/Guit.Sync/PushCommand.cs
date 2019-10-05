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
            var branch = repository.Head.TrackedBranch ?? repository.Head;
            var branchName = branch
                .FriendlyName
                .Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? string.Empty;

            var dialog = new PushDialog(branch.RemoteName ?? "origin", branchName);
            var result = mainThread.Invoke(() => dialog.ShowDialog());

            if (result == true && !string.IsNullOrEmpty(dialog.Remote))
            {
                var remote = repository
                    .Network
                    .Remotes
                    .FirstOrDefault(x => x.Name == dialog.Remote);

                if (remote != null)
                {
                    var pushRefSpec = $"refs/heads/{branchName}:refs/heads/{dialog.Branch}";
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

                    eventStream.Push(Status.Succeeded());
                }
            }

            return Task.CompletedTask;
        }
    }
}