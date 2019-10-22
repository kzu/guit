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
    [MenuCommand("Releaseator.Fetch", 'f', nameof(CherryPicker), typeof(Resources))]
    class FetchCommand : IMenuCommand
    {
        readonly IEnumerable<CherryPickConfig> repositories;
        readonly IEventStream eventStream;
        readonly CredentialsHandler credentials;
        readonly CherryPickerView view;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public FetchCommand(
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
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            foreach (var (config, index) in repositories.Select((config, index) => (config, index)))
            {
                eventStream.Push(Status.Create((index + 1) / (float)repositories.Count(), "Fetching {0}...", config.Repository.GetName()));

                config.Repository.Fetch(credentials, prune: true);
            };

            eventStream.Push(Status.Succeeded());

            mainThread.Invoke(() => view.Refresh());

            return Task.CompletedTask;
        }
    }
}