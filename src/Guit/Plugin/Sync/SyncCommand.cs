using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Merq;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    [Shared]
    [MenuCommand(nameof(Sync), Key.F2)]
    public class SyncCommand : IMenuCommand
    {
        readonly SyncView view;

        [ImportingConstructor]
        public SyncCommand(SyncView view) => this.view = view;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            Task.Run(() => Application.Run(view));
            return Task.CompletedTask;
        }
    }
}