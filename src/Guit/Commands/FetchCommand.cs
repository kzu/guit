using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Guit.Properties;
using LibGit2Sharp;
using Merq;
using Terminal.Gui;
using Git = LibGit2Sharp.Commands;

namespace Guit.Commands
{
    [Shared]
    [MenuCommand(nameof(Resources.FetchDisplayName), Key.F6)]
    public class FetchCommand : IMenuCommand
    {
        readonly Repository repository;
        readonly IEventStream eventStream;

        [ImportingConstructor]
        public FetchCommand(Repository repository, IEventStream eventStream)
        {
            this.repository = repository;
            this.eventStream = eventStream;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            foreach (var remote in repository.Network.Remotes)
            {
                eventStream.Push<StatusUpdated>($"Fetching {remote.Name}...");
                Git.Fetch(repository, remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), new FetchOptions { OnProgress = OnProgress, OnTransferProgress = OnTransferProgress }, "");
            }

            return Task.CompletedTask;
        }

        bool OnProgress(string serverProgressOutput)
        {
            eventStream.Push<StatusUpdated>(serverProgressOutput);
            return true;
        }

        bool OnTransferProgress(TransferProgress progress)
        {
            eventStream.Push<StatusUpdated>($"Received {progress.ReceivedObjects} of {progress.TotalObjects}...");
            return true;
        }
    }
}