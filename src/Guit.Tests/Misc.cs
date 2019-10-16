using System.IO;
using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Core;
using Xunit;
using Xunit.Abstractions;

namespace Guit.Tests
{
    public class Misc
    {
        ITestOutputHelper output;

        public Misc(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void ConflictFromChange()
        {
            using (var repository = new Repository(@"..\..\..\.."))
            {
                var changed = repository.RetrieveStatus(new StatusOptions()).Modified;
                foreach (var status in changed)
                {
                    var changes = repository.Diff.Compare<TreeChanges>().Modified.First(x => x.Path == status.FilePath);
                    var oldBlob = repository.Lookup<Blob>(changes.OldOid);
                    var old = repository.Lookup<Blob>(changes.OldOid);

                    output.WriteLine("#### Changed ####");
                    output.WriteLine(File.ReadAllText(Path.Combine(repository.Info.WorkingDirectory, status.FilePath)));

                    output.WriteLine("#### Unmodified ####");
                    output.WriteLine(old.GetContentText());

                    //var history = new FileHistory(repository, status.FilePath).Skip(3).First();

                    //output.WriteLine("#### Base ####");
                    //output.WriteLine(repository.Lookup<Blob>(history.Commit.Tree[status.FilePath].Target.Id).GetContentText());
                }
            }
        }

        class FakeConflict : Conflict
        {
            IndexEntry ancestor;
            IndexEntry ours;
            IndexEntry theirs;

            public FakeConflict(IndexEntry ancestor, IndexEntry ours, IndexEntry theirs)
            {
                this.ancestor = ancestor;
                this.ours = ours;
                this.theirs = theirs;
            }

            public override IndexEntry Ancestor => ancestor;

            public override IndexEntry Ours => ours;

            public override IndexEntry Theirs => theirs;

            public override string ToString() => ancestor.Path;
        }

        class FakeEntry : IndexEntry
        {
            ObjectId id;
            string path;
            StageLevel level;

            public FakeEntry(ObjectId id, string path, StageLevel level)
            {
                this.id = id;
                this.path = path;
                this.level = level;
            }

            public override ObjectId Id => id;

            public override Mode Mode => Mode.NonExecutableFile;

            public override string Path => path;

            public override StageLevel StageLevel => level;
        }

        [Fact]
        public void Test()
        {
            using (var repo = new Repository(@"..\..\..\.."))
            {
                var plugins = repo.Config.OfType<ConfigurationEntry<string>>().Where(x => x.Key == "guit.plugin").ToArray();

                repo.Config.Unset("guit.plugin");

                plugins = repo.Config.OfType<ConfigurationEntry<string>>().Where(x => x.Key == "guit.plugin").ToArray();

                // Object lookup
                //var obj = repo.Lookup("sha");
                //var commit = repo.Lookup<Commit>("sha");
                //var tree = repo.Lookup<Tree>("sha");
                //var tag = repo.Lookup<Tag>("sha");

                //// Rev walking
                //foreach (var c in repo.Commits.Walk("sha")) { }
                //var commits = repo.Commits.StartingAt("sha").Where(c => c).ToList();
                //var sortedCommits = repo.Commits.StartingAt("sha").SortBy(SortMode.Topo).ToList();

                //// Refs
                //var reference = repo.Refs["refs/heads/master"];
                //var allRefs = repo.Refs.ToList();
                //foreach (var c in repo.Refs["HEAD"].Commits) { }
                //foreach (var c in repo.Head.Commits) { }
                //var headCommit = repo.Head.Commits.First();
                //var allCommits = repo.Refs["HEAD"].Commits.ToList();
                //var newRef = repo.Refs.CreateFrom(reference);
                //var anotherNewRef = repo.Refs.CreateFrom("sha");

                //// Branches
                //// special kind of reference
                //var allBranches = repo.Branches.ToList();
                //var branch = repo.Branches["master"];
                //var remoteBranch = repo.Branches["origin/master"];
                //var localBranches = repo.Branches.Where(p => p.Type == BranchType.Local).ToList();
                //var remoteBranches = repo.Branches.Where(p => p.Type == BranchType.Remote).ToList();
                //var newBranch = repo.Branches.CreateFrom("sha");
                //var anotherNewBranch = repo.Branches.CreateFrom(newBranch);
                //repo.Branches.Delete(anotherNewBranch);

                //// Tags
                //// really another special kind of reference
                //var aTag = repo.Tags["refs/tags/v1.0"];
                //var allTags = repo.Tags.ToList();
                //var newTag = repo.Tags.CreateFrom("sha");
                //var newTag2 = repo.Tags.CreateFrom(commit);
                //var newTag3 = repo.Tags.CreateFrom(reference);
            }
        }
    }
}
