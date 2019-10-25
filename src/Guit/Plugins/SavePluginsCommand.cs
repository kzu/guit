using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Properties;

namespace Guit
{
    [Shared]
    [MenuCommand(nameof(Resources.SavePlugins), 's', nameof(Resources.Plugins), typeof(Resources), ReportProgress = false)]
    class SavePluginsCommand : IMenuCommand
    {
        readonly IShell shell;
        readonly IPluginManager manager;
        readonly PluginsView view;

        [ImportingConstructor]
        public SavePluginsCommand(IShell shell, IPluginManager manager, PluginsView view)
        {
            this.shell = shell;
            this.manager = manager;
            this.view = view;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            manager.EnabledPlugins = view.EnabledPlugins;
            shell.Shutdown();
            return Task.CompletedTask;
        }
    }
}