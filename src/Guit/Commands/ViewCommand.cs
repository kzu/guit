using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Properties;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Commands
{
    [Shared]
    [MenuCommand(nameof(Resources.ViewDisplayName), Key.F4)]
    public class ViewCommand : IMenuCommand
    {
        readonly Repository repository;
        readonly Selection selection;

        [ImportingConstructor]
        public ViewCommand(Repository repository, Selection selection)
        {
            this.repository = repository;
            this.selection = selection;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            var psi = new ProcessStartInfo("code")
            {
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            };

            switch (selection.Current)
            {
                case StatusEntry entry:
                    switch (entry.State)
                    {
                        case FileStatus.DeletedFromIndex:
                        case FileStatus.NewInIndex:
                        case FileStatus.NewInWorkdir:
                        case FileStatus.RenamedInIndex:
                        case FileStatus.RenamedInWorkdir:
                        case FileStatus.Unaltered:
                            psi.Arguments = Path.Combine(repository.Info.WorkingDirectory, entry.FilePath);
                            Process.Start(psi);
                            break;
                        case FileStatus.ModifiedInIndex:
                        case FileStatus.ModifiedInWorkdir:
                            var changes = repository.Diff.Compare<TreeChanges>().Modified.First(x => x.Path == entry.FilePath);
                            var old = repository.Lookup<Blob>(changes.OldOid);
                            // We can't show diff in this case
                            if (!changes.Exists || old.IsBinary)
                                return Task.CompletedTask;
                            // Write the old content to a temp file for comparison.
                            var tmp = Path.GetTempFileName();
                            File.WriteAllText(tmp, old.GetContentText());
                            psi.Arguments = "--diff " + tmp + " " + new FileInfo(Path.Combine(repository.Info.WorkingDirectory, entry.FilePath)).FullName;
                            Process.Start(psi);
                            break;
                        case FileStatus.Conflicted:
                            // TODO: show merge view?
                            return Task.CompletedTask;
                        default:
                            return Task.CompletedTask;
                    }
                    // TODO: error handling if "code" isn't in the PATH
                    break;
                default:
                    break;
            }

            return Task.CompletedTask;
        }
    }
}
