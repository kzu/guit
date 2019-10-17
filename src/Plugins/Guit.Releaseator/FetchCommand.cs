using System;
using System.Collections.Generic;
using System.Composition;
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
    [MenuCommand("Fetch", 'f', nameof(Releaseator))]
    class FetchCommand : IMenuCommand, IAfterExecuteCallback
    {
        readonly IEnumerable<RepositoryConfig> repositories;
        readonly IEventStream eventStream;
        readonly CredentialsHandler credentials;
        readonly ReleaseatorView view;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public FetchCommand(
            IEnumerable<RepositoryConfig> repositories, 
            IEventStream eventStream, 
            CredentialsHandler credentials, 
            ReleaseatorView view,
            MainThread mainThread)
        {
            this.repositories = repositories;
            this.eventStream = eventStream;
            this.credentials = credentials;
            this.view = view;
            this.mainThread = mainThread;
        }

        public Task AfterExecuteAsync(CancellationToken cancellation)
        {
            mainThread.Invoke(() => view.Refresh());

            return Task.CompletedTask;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var repositoriesList = repositories.ToList();

            foreach (var config in repositoriesList)
            {
                var remote = config.Repository.Network.Remotes.Single(x => x.Name == "origin");

                eventStream.Push(Status.Create((repositoriesList.IndexOf(config) + 1f) / (repositoriesList.Count + 1f), "Fetching origin {0}...", config.Repository.GetName()));

                config.Repository.Fetch(remote, credentials);
            }

            eventStream.Push(Status.Succeeded());

            return Task.CompletedTask;
        }
    }
}