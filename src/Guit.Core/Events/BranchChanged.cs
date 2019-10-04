using System;

namespace Guit.Events
{
    public class BranchChanged
    {
        public BranchChanged(string branchName) => Branch = branchName;

        public string Branch { get; }

        public static implicit operator BranchChanged(string branchName) => new BranchChanged(branchName);
    }
}