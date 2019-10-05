using System.Composition;
using System.Linq;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Merq;
using Git = LibGit2Sharp.Commands;
using System;

namespace Guit.Commands
{
    [Shared]
    [MenuCommand("Fetch", 'f', nameof(Plugin.Sync))]
    public class FetchCommand : IMenuCommand
    {
        readonly Repository repository;
        readonly IEventStream eventStream;
        readonly CredentialsHandler credentials;

        [ImportingConstructor]
        public FetchCommand(Repository repository, IEventStream eventStream, CredentialsHandler credentials)
        {
            this.repository = repository;
            this.eventStream = eventStream;
            this.credentials = credentials;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            var remotes = repository.Network.Remotes.ToList();

            foreach (var remote in remotes)
            {
                eventStream.Push(new Status($"Fetching {remote.Name}...", (remotes.IndexOf(remote) + 1f) / (remotes.Count + 1f)));

                Git.Fetch(repository, remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), new FetchOptions
                {
                    CredentialsProvider = credentials,
                    OnProgress = OnProgress,
                    OnTransferProgress = OnTransferProgress
                }, "");
            }

            eventStream.Push(new Status("Succeeded!", 1));

            return Task.CompletedTask;
        }

        bool OnProgress(string serverProgressOutput)
        {
            eventStream.Push(new Status(serverProgressOutput));
            return true;
        }

        bool OnTransferProgress(TransferProgress progress)
        {
            eventStream.Push(new Status($"Received {progress.ReceivedObjects} of {progress.TotalObjects}", progress.ReceivedObjects / (float)progress.TotalObjects));
            return true;
        }
    }
}