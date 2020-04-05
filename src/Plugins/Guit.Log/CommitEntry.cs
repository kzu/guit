using System;
using System.Diagnostics;
using LibGit2Sharp;

namespace Guit.Plugin.Log
{
    class CommitEntry : IViewPattern
    {
        public CommitEntry(IGitRepository repository, Commit commit)
        {
            Repository = repository;
            Commit = commit;
        }

        public IGitRepository Repository { get; }

        public Commit Commit { get; }

        public override int GetHashCode() => Commit.GetHashCode();

        public override bool Equals(object obj) => Commit.Equals((obj as CommitEntry)?.Commit);

        public void View() => Repository.OpenUrl(Commit);
    }
}