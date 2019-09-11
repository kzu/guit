using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using Merq;
using Terminal.Gui;
using System.Linq;
using Git = LibGit2Sharp.Commands;
using LibGit2Sharp.Handlers;

namespace Guit.Commands
{
    [Shared]
    [MenuCommand("Fetch", Key.F6, nameof(Plugin.Sync))]
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
            foreach (var remote in repository.Network.Remotes)
            {
                eventStream.Push<Status>($"Fetching {remote.Name}...");
                Git.Fetch(repository, remote.Name, remote.FetchRefSpecs.Select(x => x.Specification), new FetchOptions 
                {
                    CredentialsProvider = credentials,
                    OnProgress = OnProgress, 
                    OnTransferProgress = OnTransferProgress 
                }, "");

                for (int i = 0; i < 100; i++)
                {
                    Thread.Sleep(100);
                    eventStream.Push(new Status($"Fetching {i}", i / 100f));
                }
            }

            return Task.CompletedTask;
        }

        bool OnProgress(string serverProgressOutput)
        {
            eventStream.Push(new Status(serverProgressOutput, importance: StatusImportance.High));
            return true;
        }

        bool OnTransferProgress(TransferProgress progress)
        {
            eventStream.Push(new Status($"Received {progress.ReceivedObjects} of {progress.TotalObjects}", progress.ReceivedObjects / (float)progress.TotalObjects));
            return true;
        }
    }
}