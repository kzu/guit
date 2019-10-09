using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit
{
    [Shared]
#if DEBUG
    [MenuCommand("Plugins", Key.F10)]
#endif
    public class PluginsCommand : IMenuCommand
    {
        readonly Func<Task> run;

        [ImportingConstructor]
        public PluginsCommand(PluginsView view, IShell app) => run = () => app.RunAsync(view);

        public Task ExecuteAsync(CancellationToken cancellation) => run();
    }
}