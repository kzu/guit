using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit
{
    [Shared]
    [MenuCommand("Save", 's', "Plugins")]
    public class SavePluginsCommand : IMenuCommand
    {
        readonly IApp app;

        [ImportingConstructor]
        public SavePluginsCommand(IApp app) => this.app = app;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            // TODO: stop app
            //app.Stop();
            return Task.CompletedTask;
        }
    }
}