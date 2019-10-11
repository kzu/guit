using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Merq;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand(WellKnownCommands.ResolveConflicts, 'v', WellKnownViews.Changes, typeof(ResolveConflictsCommand))]
    public class ResolveConflictsCommand : IMenuCommand
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

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var dialog = new ResolveConflictsDialog(repository, commands);
            if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
            {
                //foreach (var resolved in dialog.GetResolvedConflicts().ToArray())
                //{
                //    repository.Index.Add(resolved.Ours.Path);
                //    repository.Index.Write();
                //}
            }

            return Task.CompletedTask;
        }
   }
}