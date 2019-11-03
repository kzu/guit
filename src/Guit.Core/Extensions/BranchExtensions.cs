using System.ComponentModel;

namespace LibGit2Sharp
{
    /// <summary>
    /// Usability overloads for <see cref="Branch"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BranchExtensions
    {
        /// <summary>
        /// Gets the FriendlyName of the branch, excluding the <see cref="Branch.RemoteName"/> if 
        /// <see cref="Branch.IsRemote"/> is <see langword="true"/>.
        /// </summary>
        public static string GetName(this Branch branch) =>
            branch.IsRemote ? branch.FriendlyName.Substring(branch.RemoteName.Length + 1) : branch.FriendlyName;

        /// <summary>
        /// Sets the <see cref="Branch.TrackedBranch"/> to the <paramref name="trackedBranch"/> 
        /// CanonicalName.
        /// </summary>
        public static void Track(this Branch branch, IRepository repository, Branch trackedBranch) =>
            repository.Branches.Update(branch, x => x.TrackedBranch = trackedBranch.CanonicalName);
    }
}