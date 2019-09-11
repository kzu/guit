using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand(nameof(Changes), Key.F1)]
    public class ChangesCommand : IMenuCommand
    {
        readonly Func<CancellationToken, Task> run;

        [ImportingConstructor]
        public ChangesCommand(ChangesView view, IApp app) => run = cancellation => app.RunAsync(view, cancellation);

        public Task ExecuteAsync(CancellationToken cancellation) => run(cancellation);
    }
}