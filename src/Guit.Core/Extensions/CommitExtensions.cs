using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace LibGit2Sharp
{
    public static class CommitExtensions
    {
        public static string GetShortSha(this Commit commit) => commit.Sha.Substring(0, 7);
    }
}