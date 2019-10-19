using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Guit.Plugin.Releaseator.Properties;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Merq;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    [MenuCommand("Releaseator.Reset", 'r', nameof(Releaseator), typeof(Resources))]
    class ResetCommand : IMenuCommand, IAfterExecuteCallback
    {
        readonly IEnumerable<ReleaseConfig> repositories;
        readonly IEventStream eventStream;
        readonly CredentialsHandler credentials;
        readonly ReleaseatorView view;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public ResetCommand(
            IEnumerable<ReleaseConfig> repositories,
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
            var dialog = new MessageBox(
                string.Format("Reset"),
                DialogBoxButton.Ok | DialogBoxButton.Cancel,
                "We will (hard) reset the current checked out branch to its corresponding remote tip");

            if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
            {
                foreach (var (config, repository, index) in repositories.Select((config, index) => (config, config.Repository, index)))
                {
                    repository.Fetch(credentials, prune: true);

                    if (repository.GetBranch(config.TargetBranchRemote) is Branch targetBranchRemote)
                    {
                        repository.Reset(ResetMode.Hard, targetBranchRemote.Tip);

                        eventStream.Push(Status.Create(
                            index + 1 / (float)repositories.Count(),
                            "Resetting {0} to {1}...", repository.GetName(), targetBranchRemote.FriendlyName));
                    }
                }

                eventStream.Push(Status.Succeeded());
            }

            return Task.CompletedTask;
        }
    }
}