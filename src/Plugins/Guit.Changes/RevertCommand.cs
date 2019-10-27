using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Guit.Plugin.Changes
{
    [Shared]
    [ChangesCommand(WellKnownCommands.Changes.Revert, 'r')]
    public class RevertCommand : IMenuCommand
    {
        readonly MainThread mainThread;
        readonly IRepository repository;
        readonly ChangesView changes;

        [ImportingConstructor]
        public RevertCommand(MainThread mainThread, IRepository repository, ChangesView changes)
        {
            this.mainThread = mainThread;
            this.repository = repository;
            this.changes = changes;
        }

        public Task ExecuteAsync(CancellationToken cancellation = default)
        {
            var entries = changes.GetMarkedEntries().ToList();

            if (entries.Any())
            {
                var dialog = new MessageBox("Revert Changes", string.Format("Are you sure you want to revert {0} item(s)?", entries.Count));
                var dialogResult = mainThread.Invoke(() => dialog.ShowDialog());

                if (dialogResult == true)
                {
                    foreach (var entry in entries)
                    {
                        switch (entry.State)
                        {
                            case FileStatus.ModifiedInWorkdir:
                                {
                                    var submodule = repository.Submodules.FirstOrDefault(x => x.Path == entry.FilePath);
                                    if (submodule != null)
                                    {
                                        using (var subRepo = new Repository(Path.Combine(repository.Info.WorkingDirectory, submodule.Path)))
                                            subRepo.Reset(ResetMode.Hard, subRepo.Head.Tip);

                                        repository.Submodules.Update(submodule.Name, new SubmoduleUpdateOptions());
                                    }
                                    else
                                    {
                                        repository.RevertFileChanges(entry.FilePath);
                                    }
                                    break;
                                }
                            case FileStatus.NewInWorkdir: repository.Remove(entry.FilePath); break;
                        }
                    }

                    changes.Refresh();
                }
            }

            return Task.CompletedTask;
        }
    }
}