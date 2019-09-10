using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using Merq;
using Terminal.Gui;
using System.Linq;
using Git = LibGit2Sharp.Commands;

namespace Guit.Commands
{
    [Shared]
    [MenuCommand("Fetch", Key.F6, nameof(Plugin.Sync))]
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
                eventStream.Push<Status>($"Fetching {remote.Name}...");
                Git.Fetch(repository, remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), new FetchOptions { OnProgress = OnProgress, OnTransferProgress = OnTransferProgress }, "");
            }

            return Task.CompletedTask;
        }

        bool OnProgress(string serverProgressOutput)
        {
            eventStream.Push<Status>(serverProgressOutput);
            return true;
        }

        bool OnTransferProgress(TransferProgress progress)
        {
            eventStream.Push<Status>(progress.ReceivedObjects / (float)progress.TotalObjects);
            return true;
        }
    }
}