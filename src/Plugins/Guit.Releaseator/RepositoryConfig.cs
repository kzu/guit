using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace Guit.Plugin.Releaseator
{
    class RepositoryConfig
    {
        public RepositoryConfig(IRepository repository, string baseBranch, string targetBranch, string mergeBranchSuffix = "/merge")
        {
            Repository = repository;
            BaseBranch = baseBranch;
            ReleaseBranch = targetBranch;
            MergeBranch = targetBranch + mergeBranchSuffix;
        }

        public IRepository Repository { get; set; }

        public string BaseBranch { get; }

        public string? BaseBranchSha { get; set; }

        public string ReleaseBranch { get; }

        public string? ReleaseBranchSha { get; set; }

        public string MergeBranch { get; set; }

        public string[]? IgnoreCommits { get; set; }
    }
}
