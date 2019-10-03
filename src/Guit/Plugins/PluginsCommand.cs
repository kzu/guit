using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit
{
    [Shared]
    [MenuCommand("Plugins", Key.F10)]
    public class PluginsCommand : IMenuCommand
    {
        readonly Func<Task> run;

        [ImportingConstructor]
        public PluginsCommand(PluginsView view, IApp app) => run = () => app.RunAsync(view);

        public Task ExecuteAsync(CancellationToken cancellation) => run();
    }
}