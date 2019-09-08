using System.ComponentModel;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using DotNetGit.Events;
using DotNetGit.Properties;
using Merq;
using Terminal.Gui;

namespace DotNetGit.Commands
{
    [Shared]
    [MenuCommand(nameof(Resources.FetchDisplayName), Key.F6, 1)]
    public class FetchCommand : IMenuCommand
    {
        IEventStream eventStream;

        [ImportingConstructor]
        public FetchCommand(IEventStream eventStream) => this.eventStream = eventStream;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            eventStream.Push<StatusUpdated>("Fetched!");
            return Task.CompletedTask;
        }
    }
}