using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Merq;

namespace Guit.Plugin.Sync
{
    [Shared]
    [SyncCommand(WellKnownCommands.Sync.Fetch, 'f')]
    class FetchCommand : IMenuCommand
    {
        readonly IRepository repository;
        readonly IEventStream eventStream;
        readonly CredentialsHandler credentials;

        [ImportingConstructor]
        public FetchCommand(IRepository repository, IEventStream eventStream, CredentialsHandler credentials)
        {
            this.repository = repository;
            this.eventStream = eventStream;
            this.credentials = credentials;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var remotes = repository.Network.Remotes.ToList();

            foreach (var remote in remotes)
            {
                eventStream.Push(Status.Create((remotes.IndexOf(remote) + 1f) / (remotes.Count + 1f), "Fetching {0}...", remote.Name));

                repository.Fetch(remote, credentials, eventStream);
            }

            eventStream.Push(Status.Succeeded());

            return Task.CompletedTask;
        }
    }
}