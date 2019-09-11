using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Merq;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    [Shared]
    [MenuCommand("Sync.Pull", Key.F7, nameof(Sync))]
    public class PullCommand : IMenuCommand
    {
        IEventStream eventStream;

        [ImportingConstructor]
        public PullCommand(IEventStream eventStream) => this.eventStream = eventStream;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            eventStream.Push<Status>("Pulled!");
            return Task.CompletedTask;
        }
    }
}