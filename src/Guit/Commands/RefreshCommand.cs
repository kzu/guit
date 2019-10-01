using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand("Refresh", Key.F5, Visible = false)]
    public class RefreshCommand : IMenuCommand
    {
        readonly IApp app;

        [ImportingConstructor]
        public RefreshCommand(IApp app) => this.app = app;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            app.Current?.Refresh();

            return Task.CompletedTask;
        }
    }
}