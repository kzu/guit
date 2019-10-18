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
        private readonly ICommandService commandService;

        [ImportingConstructor]
        public ResetCommand(
            IEnumerable<ReleaseConfig> repositories,
            IEventStream eventStream,
            CredentialsHandler credentials,
            ReleaseatorView view,
            MainThread mainThread,
            ICommandService commandService)
        {
            this.repositories = repositories;
            this.eventStream = eventStream;
            this.credentials = credentials;
            this.view = view;
            this.mainThread = mainThread;
            this.commandService = commandService;
        }

        public Task AfterExecuteAsync(CancellationToken cancellation)
        {
            mainThread.Invoke(() => view.Refresh());

            return Task.CompletedTask;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var dialog = new MessageBox(
                string.Format("Reset All Merge Branches"),
                DialogBoxButton.Ok | DialogBoxButton.Cancel,
                "We will (hard) reset all merge branches to its corresponding remote or release branch tip");

            if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
            {
                var repositoriesList = repositories.ToList();

                foreach (var config in repositories)
                {
                    var repository = config.Repository;

                    var mergeBranch = config.Repository.SwitchToMergeBranch(config);

                    eventStream.Push(Status.Create(
                        (repositoriesList.IndexOf(config) + 1f) / (repositoriesList.Count + 1f),
                        "Reset {0}/{1}", config.Repository.GetName(), mergeBranch.GetName()));

                    repository.Fetch(repository.Network.Remotes, credentials, prune: true);

                    var releaseBranch = repository.Branches.Single(x => x.FriendlyName == "origin/" + config.ReleaseBranch);

                    repository.Reset(ResetMode.Hard, releaseBranch.Tip);
                }

                eventStream.Push(Status.Succeeded());
            }

            return Task.CompletedTask;
        }
    }
}