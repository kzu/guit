using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Guit.Plugin.CherryPicker.Properties;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Merq;

namespace Guit.Plugin.CherryPicker
{
    [Shared]
    [MenuCommand("Releaseator.Reset", 'r', nameof(CherryPicker), typeof(Resources), IsDynamic = true)]
    class ResetCommand : IDynamicMenuCommand
    {
        readonly IEnumerable<CherryPickConfig> repositories;
        readonly IEventStream eventStream;
        readonly CredentialsHandler credentials;
        readonly CherryPickerView view;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public ResetCommand(
            IEnumerable<CherryPickConfig> repositories,
            IEventStream eventStream,
            CredentialsHandler credentials,
            CherryPickerView view,
            MainThread mainThread)
        {
            this.repositories = repositories;
            this.eventStream = eventStream;
            this.credentials = credentials;
            this.view = view;
            this.mainThread = mainThread;

            IsVisible = IsEnabled = repositories.Count() > 1;
        }

        public bool IsVisible { get; }

        public bool IsEnabled { get; }

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
                            (index + 1) / (float)repositories.Count(),
                            "Resetting {0} to {1}...", repository.GetName(), targetBranchRemote.FriendlyName));
                    }
                }

                eventStream.Push(Status.Succeeded());

                mainThread.Invoke(() => view.Refresh());
            }

            return Task.CompletedTask;
        }
    }
}