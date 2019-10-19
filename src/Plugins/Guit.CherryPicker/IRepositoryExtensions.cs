using System;
using System.IO;
using System.Linq;
using Guit.Plugin.Releaseator;

namespace LibGit2Sharp
{
    static class IRepositoryExtensions
    {
        const string GitSuffix = ".git";

        public static string GetName(this IRepository repository) => new DirectoryInfo(repository.Info.WorkingDirectory).Name;

        public static string GetRepoUrl(this IRepository repository)
        {
            var repoUrl = repository.Config.GetValueOrDefault<string>("remote.origin.url");
            if (repoUrl.EndsWith(GitSuffix))
                repoUrl = repoUrl.Remove(repoUrl.Length - GitSuffix.Length);

            return repoUrl;
        }

        public static Branch GetBranch(this IRepository repository, string branchFriendlyName) =>
            repository.Branches.FirstOrDefault(x => x.FriendlyName == branchFriendlyName);

        static void GetLocalAndRemoteBranch(this IRepository repository, string localBranchName, string remoteBranchName, out Branch? localBranch, out Branch? remoteBranch)
        {
            localBranch = repository.GetBranch(localBranchName);
            remoteBranch = repository.GetBranch(remoteBranchName);
        }

        public static Branch GetBaseBranch(this IRepository repository, ReleaseConfig config)
        {
            repository.GetLocalAndRemoteBranch(config.BaseBranch, config.BaseBranchRemote, out var localBranch, out var remoteBranch);

            if (localBranch is null && remoteBranch is null)
                throw new InvalidOperationException(string.Format("Branch '{0}' not found", config.BaseBranch));

#pragma warning disable CS8603 // Possible null reference return.
            return localBranch ?? remoteBranch;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static Branch SwitchToTargetBranch(this IRepository repository, ReleaseConfig config)
        {
            GetLocalAndRemoteBranch(repository, config.TargetBranch, config.TargetBranchRemote, out var targetBranch, out var targetBranchRemote);

            if (targetBranch == null && targetBranchRemote != null)
                targetBranch = repository.CreateBranch(config.TargetBranch, targetBranchRemote.Tip);

            if (targetBranch is null)
                throw new InvalidOperationException(string.Format("Branch {0} not found", config.TargetBranch));

            // Checkout target branch
            repository.Checkout(targetBranch);

            if (config.SyncTargetBranch && targetBranchRemote != null)
            {
                try
                {
                    // And try pull with fast forward from remote
                    repository.Merge(
                        targetBranchRemote,
                        repository.Config.BuildSignature(DateTimeOffset.Now),
                        new MergeOptions { FastForwardStrategy = FastForwardStrategy.FastForwardOnly });
                }
                catch (NonFastForwardException) { }
            }

            return targetBranch;
        }
    }
}