using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Merq;

namespace Guit.Plugin.Changes
{
    [Shared]
    [ChangesCommand(WellKnownCommands.ResolveConflicts, 'v', DefaultVisible = false, IsDynamic = true)]
    public class ResolveConflictsCommand : IDynamicMenuCommand
    {
        readonly IRepository repository;
        readonly ICommandService commands;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public ResolveConflictsCommand(IRepository repository, ICommandService commands, MainThread mainThread)
        {
            this.repository = repository;
            this.commands = commands;
            this.mainThread = mainThread;
        }

        public bool IsVisible => repository.Index.Conflicts.Any();

        public bool IsEnabled => repository.Index.Conflicts.Any();

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            mainThread.Invoke(() => new ResolveConflictsDialog(repository, commands).ShowDialog());
            return Task.CompletedTask;
        }
    }
}