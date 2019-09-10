using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Guit.Properties;
using Merq;
using Terminal.Gui;

namespace Guit.Commands
{
    [Shared]
    [MenuCommand(nameof(Resources.PullDisplayName), Key.F5)]
    public class PullCommand : IMenuCommand
    {
        IEventStream eventStream;

        [ImportingConstructor]
        public PullCommand(IEventStream eventStream) => this.eventStream = eventStream;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            eventStream.Push<StatusUpdated>("Pulled!");
            return Task.CompletedTask;
        }
    }
}