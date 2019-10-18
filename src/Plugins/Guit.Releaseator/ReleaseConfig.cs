using LibGit2Sharp;

namespace Guit.Plugin.Releaseator
{
    class ReleaseConfig
    {
        public ReleaseConfig(IRepository repository, string baseBranch, string releaseBranch, string mergeBranchSuffix = "/merge")
        {
            Repository = repository;
            BaseBranch = baseBranch;
            ReleaseBranch = releaseBranch;
            MergeBranch = releaseBranch + mergeBranchSuffix;
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
