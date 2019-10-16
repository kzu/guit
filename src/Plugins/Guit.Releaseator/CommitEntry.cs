using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace Guit.Plugin.Releaseator
{
    class CommitEntry
    {
        public CommitEntry(IRepository repository, Commit commit)
        {
            Repository = repository;
            Commit = commit;
        }

        public IRepository Repository { get; }

        public Commit Commit { get; }
    }
}