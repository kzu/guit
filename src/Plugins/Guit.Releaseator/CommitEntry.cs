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

        public override int GetHashCode() => Commit.GetHashCode();

        public override bool Equals(object obj) => Commit.Equals((obj as CommitEntry)?.Commit);
    }
}