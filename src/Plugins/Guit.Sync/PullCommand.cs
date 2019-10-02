using System;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Sync.Properties;
using LibGit2Sharp;

namespace Guit.Plugin.Sync
{
    [Shared]
    [MenuCommand("Sync.Pull", 'p', nameof(Sync), typeof(Resources))]
    public class PullCommand : IMenuCommand
    {
        readonly MainThread mainThread;
        readonly IRepository repository;

        [ImportingConstructor]
        public PullCommand(MainThread mainThread, IRepository repository)
        {
            this.mainThread = mainThread;
            this.repository = repository;
        }

        public Task ExecuteAsync(CancellationToken cancellation)
        {
            var branch = repository.Head.TrackedBranch ?? repository.Head;
            var remote = branch.RemoteName ?? "origin";
            var branchName = branch
                .FriendlyName
                .Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault() ?? string.Empty;

            var dialog = new PullDialog(remote, branchName, true);
            var result = mainThread.Invoke(() => dialog.ShowDialog());

            if (result == true && !string.IsNullOrEmpty(dialog.Remote) && !string.IsNullOrEmpty(dialog.Branch))
            {
                var targetBranch = repository
                    .Branches
                    .FirstOrDefault(x =>
                        x.RemoteName == dialog.Remote &&
                        x.CanonicalName.EndsWith(dialog.Branch));

                if (targetBranch != null)
                {
                    repository.Merge(
                        targetBranch,
                        repository.Config.BuildSignature(DateTimeOffset.Now),
                        new MergeOptions()
                        {
                            FastForwardStrategy = dialog.IsFastForward ?
                                FastForwardStrategy.FastForwardOnly : FastForwardStrategy.NoFastForward
                        });
                }
            }

            return Task.CompletedTask;
        }
    }
}