using System;
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
        readonly Func<Task> run;

        [ImportingConstructor]
        public LogCommand(LogView view, IApp app) => run = () => app.RunAsync(view);

        public Task ExecuteAsync(CancellationToken cancellation) => run();
    }
}