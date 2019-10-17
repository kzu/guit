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
        readonly IPluginManager manager;
        readonly PluginsView view;

        [ImportingConstructor]
        public SavePluginsCommand(IShell app, IPluginManager manager, PluginsView view)
        {
            this.app = app;
            this.manager = manager;
            this.view = view;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            manager.EnabledPlugins = view.EnabledPlugins;
            app.Shutdown();
            return Task.CompletedTask;
        }
    }
}