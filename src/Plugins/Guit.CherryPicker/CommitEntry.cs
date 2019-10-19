using LibGit2Sharp;

namespace Guit.Plugin.CherryPicker
{
    class CommitEntry
    {
        public CommitEntry(ReleaseConfig config, Commit commit)
        {
            Config = config;
            Commit = commit;
        }

        public ReleaseConfig Config { get; }

        public Commit Commit { get; }

        public override int GetHashCode() => Commit.GetHashCode();

        public override bool Equals(object obj) => Commit.Equals((obj as CommitEntry)?.Commit);
    }
}