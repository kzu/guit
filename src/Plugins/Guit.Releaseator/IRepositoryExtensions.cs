using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

        public static Branch? SwitchToMergeBranch(this IRepository repository, RepositoryConfig config, bool createMergeBranch = false)
        {
            // Search the local merge branch
            var mergeBranch = repository.Branches.FirstOrDefault(x => x.FriendlyName == config.MergeBranch);

            if (mergeBranch is null && !createMergeBranch)
                return default;

            var remoteMergeBranch = repository.Branches.FirstOrDefault(x => x.FriendlyName == "origin/" + config.MergeBranch && x.IsRemote);
            var releaseBranch = repository.Branches.Single(x => x.FriendlyName == "origin/" + config.ReleaseBranch);

            if (mergeBranch == null)
            {
                // Checkout the remote merge branch or the release by default
                repository.Checkout(repository.Branches.FirstOrDefault(x => x.FriendlyName == "origin/" + config.MergeBranch) ?? releaseBranch);

                // Create the merge branch from the previously checked out branch (remote merge or release)
                mergeBranch = repository.CreateBranch(config.MergeBranch);
            }

            // Checkout merge branch
            repository.Checkout(mergeBranch);

            // And try pull with fast forward from remote
            if (remoteMergeBranch != null)
            {
                try
                {
                    repository.Merge(
                        remoteMergeBranch,
                        repository.Config.BuildSignature(DateTimeOffset.Now),
                        new MergeOptions { FastForwardStrategy = FastForwardStrategy.FastForwardOnly });
                }
                catch (NonFastForwardException) { }
            }

            return mergeBranch;
        }
    }
}