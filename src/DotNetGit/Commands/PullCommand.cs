using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Merq;
using Terminal.Gui;

namespace DotNetGit.Commands
{
    [Shared]
    [Export]
    [Export(typeof(IMainCommand))]
    [ExportMetadata("HotKey", Key.F5)]
    [ExportMetadata("DisplayName", "Pull")]
    public class PullCommand : IMainCommand
    {
        IEventStream eventStream;

        [ImportingConstructor]
        public PullCommand(IEventStream eventStream) => this.eventStream = eventStream;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            eventStream.Push("Pulled!");
            return Task.CompletedTask;
        }
    }
}