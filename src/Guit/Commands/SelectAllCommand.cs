using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand("SelectAll", '*', Visible = false)]
    public class SelectAllCommand : IMenuCommand
    {
        readonly IApp app;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public SelectAllCommand(IApp app, MainThread mainThread)
        {
            this.app = app;
            this.mainThread = mainThread;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            mainThread.Invoke(() => app.Current?.SelectAll(invertSelection: true));

            return Task.CompletedTask;
        }
    }
}