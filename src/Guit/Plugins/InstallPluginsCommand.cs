using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Guit.Properties;

namespace Guit
{
    [Shared]
    [MenuCommand(nameof(Resources.InstallPlugins), 'i', nameof(Resources.Plugins), typeof(Resources), ReportProgress = false)]
    class InstallPluginsCommand : IMenuCommand
    {
        readonly IShell shell;
        readonly IPluginManager plugins;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public InstallPluginsCommand(IShell shell, IPluginManager plugins, MainThread mainThread)
        {
            this.shell = shell;
            this.plugins = plugins;
            this.mainThread = mainThread;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var dialog = new InstallPluginsDialog(plugins);

            if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
            {
                var selected = dialog.GetMarkedEntries();
                var installed = false;
                foreach (var plugin in selected)
                {
                    plugins.Install(plugin.Identity.Id, plugin.Identity.Version.ToNormalizedString());
                    installed = true;
                }

                if (installed)
                    shell.Shutdown();
            }

            return Task.CompletedTask;
        }
    }
}
