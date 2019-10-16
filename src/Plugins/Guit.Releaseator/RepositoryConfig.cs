using System;
using System.Collections.Generic;
using System.Text;

namespace Guit.Plugin.Releaseator
{
    class RepositoryConfig
    {
        public RepositoryConfig(string baseBranch, string targetBranch)
        {
            BaseBranch = baseBranch;
            TargetBranch = targetBranch;
        }

        public string BaseBranch { get; }

        public string? BaseBranchSha { get; set; }

        public string TargetBranch { get; }

        public string? TargetBranchSha { get; set; }

        public string[]? IgnoreCommits { get; set; }
    }
}
