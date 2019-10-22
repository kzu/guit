using System.Diagnostics;
using LibGit2Sharp;

namespace Guit.Plugin.CherryPicker
{
    class CommitEntry : IViewPattern
    {
        public CommitEntry(CherryPickConfig config, Commit commit)
        {
            Config = config;
            Commit = commit;
        }

        public CherryPickConfig Config { get; }

        public Commit Commit { get; }

        public override int GetHashCode() => Commit.GetHashCode();

        public override bool Equals(object obj) => Commit.Equals((obj as CommitEntry)?.Commit);

        public void View() => Config.Repository.OpenUrl(Commit);
    }
}