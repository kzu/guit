using Merq;

namespace Guit.Events
{
    /// <summary>
    /// Event raised via the <see cref="IEventStream"/> to signal that 
    /// the current branch has changed.
    /// </summary>
    public class BranchChanged
    {
        /// <summary>
        /// Initializes the event with the new branch name.
        /// </summary>
        public BranchChanged(string branchName) => Branch = branchName;

        /// <summary>
        /// Name of the branch.
        /// </summary>
        public string Branch { get; }

        /// <summary>
        /// Provides implicit conversion from a branch name string to 
        /// the <see cref="BranchChanged"/> type.
        /// </summary>
        public static implicit operator BranchChanged(string branchName) => new BranchChanged(branchName);
    }
}