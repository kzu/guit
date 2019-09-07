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
    [ExportMetadata("HotKey", Key.F6)]
    [ExportMetadata("DisplayName", "Fetch")]
    public class FetchCommand : IMainCommand
    {
        IEventStream eventStream;

        [ImportingConstructor]
        public FetchCommand(IEventStream eventStream) => this.eventStream = eventStream;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            eventStream.Push("Fetched!");
            return Task.CompletedTask;
        }
    }
}