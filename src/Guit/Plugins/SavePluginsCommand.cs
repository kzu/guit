using System.Composition;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace Guit
{
    [Shared]
    [MenuCommand("Save", 's', "Plugins", ReportProgress = false)]
    public class SavePluginsCommand : IMenuCommand
    {
        readonly IShell app;

        [ImportingConstructor]
        public SavePluginsCommand(IShell app) => this.app = app;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            // TODO: save selected plugins list.

            app.Shutdown();
            return Task.CompletedTask;
        }
    }
}