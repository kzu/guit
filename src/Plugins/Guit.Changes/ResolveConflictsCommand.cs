using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Plugin.Changes.Properties;
using LibGit2Sharp;
using Merq;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand(WellKnownCommands.ResolveConflicts, 'c', WellKnownViews.Changes, typeof(Resources), IsDynamic = true)]
    public class ResolveConflictsCommand : IDynamicMenuCommand, IAfterExecuteCallback
    {
        readonly IRepository repository;
        readonly ICommandService commands;
        readonly MainThread mainThread;
        readonly IShell shell;

        [ImportingConstructor]
        public ResolveConflictsCommand(IRepository repository, ICommandService commands, MainThread mainThread, IShell shell)
        {
            this.repository = repository;
            this.commands = commands;
            this.mainThread = mainThread;
            this.shell = shell;
        }

        public bool IsVisible => repository.Index.Conflicts.Any();

        public bool IsEnabled => repository.Index.Conflicts.Any();

        public Task AfterExecuteAsync(CancellationToken cancellation)
        {
            mainThread.Invoke(() => shell.Refresh());
            return Task.CompletedTask;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            mainThread.Invoke(() => new ResolveConflictsDialog(repository, commands).ShowDialog());
            return Task.CompletedTask;
        }
   }
}