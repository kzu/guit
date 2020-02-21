using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace Guit.Plugin.CherryPicker
{
    class CherryPickConfig
    {
        string? targetBranch;

        public CherryPickConfig(IRepository repository, string? baseBranch = default, string? targetBranch = default)
        {
            Repository = repository;

            BaseBranch = baseBranch;
            this.targetBranch = targetBranch;
        }

        public IRepository Repository { get; set; }

        public string? BaseBranch { get; set; }

        public string? BaseBranchRemote => BaseBranch is null ? default : "origin/" + BaseBranch;

        public string TargetBranch => targetBranch ?? Repository.Head.GetName();

        public string TargetBranchRemote => "origin/" + TargetBranch;

        /// <summary>
        /// Gets the list of commit strings to be ignored
        /// Current supported values are: MessageShort (StartWith) and Sha
        /// </summary>
        public IEnumerable<string> IgnoreCommits { get; set; } = Enumerable.Empty<string>();

        /// <summary>
        /// <see langword="true"/> if the local target branch should be automatically
        /// merged (Pull FastForward) with the remote 
        /// </summary>
        public bool SyncTargetBranch { get; set; }
    }
}