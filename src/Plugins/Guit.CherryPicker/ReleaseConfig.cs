using LibGit2Sharp;

namespace Guit.Plugin.CherryPicker
{
    class ReleaseConfig
    {
        public ReleaseConfig(IRepository repository, string baseBranch, string targetBranch)
        {
            Repository = repository;

            BaseBranch = baseBranch;
            TargetBranch = targetBranch;
        }

        public IRepository Repository { get; set; }

        public string BaseBranch { get; }

        public string BaseBranchRemote => "origin/" + BaseBranch;

        public string TargetBranch { get; }

        public string TargetBranchRemote => "origin/" + TargetBranch;

        /// <summary>
        /// Gets the list of commit strings to be ignored
        /// Current supported values are: MessageShort (StartWith) and Sha
        /// </summary>
        public string[]? IgnoreCommits { get; set; }

        /// <summary>
        /// Gets the commits count limit to be evaluated
        /// </summary>
        public int Limit { get; set; } = 500;

        /// <summary>
        /// Gets true of the local target branch should be automatically
        /// merged (Pull FastFordward) with the remote 
        /// </summary>
        public bool SyncTargetBranch { get; } = true;
    }
}