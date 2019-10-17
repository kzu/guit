using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace Guit.Plugin.Releaseator
{
    class CommitEntry
    {
        public CommitEntry(RepositoryConfig config, Commit commit)
        {
            Config = config;
            Commit = commit;
        }

        public RepositoryConfig Config { get; }

        public Commit Commit { get; }
    }
}