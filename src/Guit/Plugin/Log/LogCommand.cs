using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit.Plugin.Log
{
    [Shared]
    [MenuCommand(nameof(Log), Key.F3)]
    public class LogCommand : IMenuCommand
    {
        readonly LogView view;

        [ImportingConstructor]
        public LogCommand(LogView view) => this.view = view;

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            Task.Run(() => Application.Run(view));
            return Task.CompletedTask;
        }
    }
}