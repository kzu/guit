using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    [MenuCommand("Ignore", 'i', nameof(Releaseator))]
    class IgnoreCommand : IMenuCommand, IAfterExecuteCallback
    {


        readonly MainThread mainThread;
        readonly ReleaseatorView view;

        [ImportingConstructor]
        public IgnoreCommand(MainThread mainThread, ReleaseatorView view)
        {
            this.mainThread = mainThread;
            this.view = view;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var entries = view.MarkedEntries.ToList();

            var dialog = new MessageBox(
                string.Format("Ignore Commits"),
                DialogBoxButton.Ok | DialogBoxButton.Cancel,
                string.Format("We will ignore the selected {0} commit(s)", entries.Count()));

            if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
            {
                var lines = entries.Select(x =>
                    string.Format(
                        "{0}\t{1}\t{2}\t{3}",
                        string.Format("[{0}]({1})",
                            x.Config.Repository.GetRepoUrl().Replace("https://github.com/", string.Empty) + "@" + x.Commit.Sha.Substring(0, 7),
                            x.Config.Repository.GetRepoUrl() + "/commit/" + x.Commit.Sha),
                        x.Commit.Author.Name,
                        x.Commit.MessageShort,
                        x.Commit.Sha));

                File.AppendAllLines(Constants.NoReleaseFile, lines);
            }

            return Task.CompletedTask;
        }

        public Task AfterExecuteAsync(CancellationToken cancellation)
        {
            mainThread.Invoke(() => view.Refresh());

            return Task.CompletedTask;
        }
    }
}