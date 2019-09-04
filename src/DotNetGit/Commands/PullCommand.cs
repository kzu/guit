using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Merq;
using Terminal.Gui;

namespace DotNetGit.Commands
{
    public class PullCommand : IAsyncCommand
    {
    }

    [Shared]
    [Export]
    [Export(typeof(IMainCommand))]
    [ExportMetadata("HotKey", Key.F5)]
    [ExportMetadata("DisplayName", "Pull")]
    public class PullCommandHandler : IAsyncCommandHandler<FetchCommand>, IMainCommand
    {
        public bool CanExecute(FetchCommand command) => true;

        public Task ExecuteAsync(FetchCommand command, CancellationToken cancellation)
        {
            return Task.CompletedTask;
        }
    }
}