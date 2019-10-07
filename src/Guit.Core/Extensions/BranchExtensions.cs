using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace LibGit2Sharp
{
    public static class BranchExtensions
    {
        public static string GetName(this Branch branch) =>
            branch.IsRemote ? branch.FriendlyName.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Last() : branch.FriendlyName;

        public static void Track(this Branch branch, IRepository repository, Branch trackedBranch) =>
            repository.Branches.Update(branch, x => x.TrackedBranch = trackedBranch.CanonicalName);
    }
}