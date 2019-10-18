using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    [Export]
    [ContentView(nameof(Releaseator), '4')]
    class ReleaseatorView : ContentView
    {
        readonly IEnumerable<ReleaseConfig> repositories;
        readonly CredentialsHandler credentials;
        readonly ListView<CommitEntry> view;

        [ImportingConstructor]
        public ReleaseatorView(IEnumerable<ReleaseConfig> repositories, CredentialsHandler credentials)
            : base(nameof(Releaseator))
        {
            this.repositories = repositories;
            this.credentials = credentials;

            view = new ListView<CommitEntry>(
                new ColumnDefinition<CommitEntry>(x => x.Config.Repository.GetName(), 30),
                new ColumnDefinition<CommitEntry>(x => x.Commit.Sha.Substring(0, 7), 10),
                new ColumnDefinition<CommitEntry>(x => x.Commit.MessageShort, "*"),
                new ColumnDefinition<CommitEntry>(x => x.Commit.Author.Name, 15),
                new ColumnDefinition<CommitEntry>(x => x.Commit.Committer.When.ToString("g"), 20))
            {
                AllowsMarking = true
            };

            Content = view;
        }
        public IEnumerable<CommitEntry> MarkedEntries =>
            view.Values
                .Where((x, i) => view.Source.IsMarked(i))
                .Select(x => x);

        public CommitEntry? SelectedEntry =>
            view.SelectedItem >= 0 && view.SelectedItem < view.Values.Count() ?
                view.Values.ElementAt(view.SelectedItem) : null;

        public override void Refresh()
        {
            base.Refresh();

            var commits = new List<CommitEntry>();

            foreach (var config in repositories)
            {
                var repository = config.Repository;

                var ignoredCommits = new List<string>();
                if (File.Exists(Constants.NoReleaseFile))
                {
                    ignoredCommits = File
                        .ReadAllLines(Constants.NoReleaseFile)
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => x.Split('\t').LastOrDefault())
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Distinct()
                        .ToList();
                }

                var baseCommits = GetCommits(repository, config.BaseBranch, config.BaseBranchSha, config.IgnoreCommits, 500).ToList();
                var releaseCommits = GetCommits(repository, config.ReleaseBranch, config.ReleaseBranchSha, config.IgnoreCommits, 1000).ToList();

                var mergeBranch = repository.SwitchToMergeBranch(config);
                var mergeCommits = GetCommits(mergeBranch, null, new string[0]).ToList();

                var missingCommits = baseCommits
                    .Where(baseCommit =>
                        !releaseCommits.Any(releaseCommit => string.Compare(baseCommit.Message, releaseCommit.Message, CultureInfo.CurrentCulture, System.Globalization.CompareOptions.IgnoreSymbols) == 0) &&
                        !mergeCommits.Any(mergeCommit => string.Compare(baseCommit.Message, mergeCommit.Message, CultureInfo.CurrentCulture, System.Globalization.CompareOptions.IgnoreSymbols) == 0) &&
                        !ignoredCommits.Contains(baseCommit.Sha));

                commits.AddRange(missingCommits.Select(x => new CommitEntry(config, x)));
            }

            view.SetValues(commits);
        }

        IEnumerable<Commit> GetCommits(IRepository repository, string branchName, string? sha, string[]? commitMessagesToBeIgnored, int count = 500) =>
            // TODO: maybe FirstOrDefault? Or better error/warnings?
            GetCommits(repository.Branches.SingleOrDefault(x => x.FriendlyName == "origin/" + branchName), sha, commitMessagesToBeIgnored ?? new string[0], count);

        IEnumerable<Commit> GetCommits(Branch branch, string? sha, string[] commitMessagesToBeIgnored, int count = 500)
        {
            if (branch == null)
                yield break;

            foreach (var commit in branch.Commits)
            {
                if (!commitMessagesToBeIgnored.Any(x => commit.MessageShort.StartsWith(x)))
                {
                    count--;
                    yield return commit;
                }

                if (commit.Sha == sha || count <= 0)
                    break;
            }
        }
    }
}
