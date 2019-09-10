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
        readonly ChangesView view;

        [ImportingConstructor]
        public ChangesCommand(ChangesView view) => this.view = view;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            Task.Run(() => Application.Run(view));
            return Task.CompletedTask;
        }
    }
}