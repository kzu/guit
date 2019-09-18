using System;
using System.Linq;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Merq;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    [Shared]
    [MenuCommand("Sync.Pull", Key.F7, nameof(Sync))]
    public class PullCommand : IMenuCommand
    {
        readonly ThreadContext threadContext;
        readonly Repository repository;

        [ImportingConstructor]
        public PullCommand(ThreadContext threadContext, Repository repository)
        {
            this.threadContext = threadContext;
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
            var result = threadContext.MainThread.Invoke(() => dialog.ShowDialog());

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