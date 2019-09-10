using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Merq;
using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand("Commit", Key.F6, nameof(Changes))]
    public class CommitCommand : IMenuCommand
    {
        IEventStream eventStream;

        [ImportingConstructor]
        public CommitCommand(IEventStream eventStream) => this.eventStream = eventStream;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            eventStream.Push<StatusUpdated>("Committed!");
            return Task.CompletedTask;
        }
    }
}