using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    [Shared]
    [MenuCommand(nameof(Sync), Key.F2)]
    public class SyncCommand : IMenuCommand
    {
        readonly Func<Task> run;

        [ImportingConstructor]
        public SyncCommand(SyncView view, IApp app) => run = () => app.RunAsync(view);

        public Task ExecuteAsync(CancellationToken cancellation) => run();
    }
}