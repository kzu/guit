using System;
using System.Linq;
using System.Collections.Generic;
using System.Composition;
using LibGit2Sharp;
using Terminal.Gui;
using LibGit2Sharp.Handlers;
using System.IO;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    [Export]
    [ContentView(nameof(Releaseator), '4')]
    class ReleaseatorView : ContentView
    {
        readonly IEnumerable<RepositoryConfig> repositories;
        readonly CredentialsHandler credentials;
        readonly ListView<CommitEntry> view;

        [ImportingConstructor]
        public ReleaseatorView(IEnumerable<RepositoryConfig> repositories, CredentialsHandler credentials)
            : base(nameof(Releaseator))
        {
            this.repositories = repositories;
            this.credentials = credentials;

            view = new ListView<CommitEntry>(
                new ListViewItemSelector<CommitEntry>(x => x.Config.Repository.GetName(), 30),
                new ListViewItemSelector<CommitEntry>(x => x.Commit.Sha.Substring(0, 7), 10),
                new ListViewItemSelector<CommitEntry>(x => x.Commit.MessageShort, "*"),
                new ListViewItemSelector<CommitEntry>(x => x.Commit.Author.Name, 15),
                new ListViewItemSelector<CommitEntry>(x => x.Commit.Committer.When.ToString("g"), 20))
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
                        !releaseCommits.Any(targetCommit => baseCommit.Message == targetCommit.Message) &&
                        !mergeCommits.Any(mergeCommit => baseCommit.Message == mergeCommit.Message) &&
                        !ignoredCommits.Contains(baseCommit.Sha));

                commits.AddRange(missingCommits.Select(x => new CommitEntry(config, x)));
            }

            view.SetValues(commits);
        }

        IEnumerable<Commit> GetCommits(IRepository repository, string branchName, string? sha, string[]? commitMessagesToBeIgnored, int count = 500) =>
            GetCommits(repository.Branches.Single(x => x.FriendlyName == "origin/" + branchName), sha, commitMessagesToBeIgnored ?? new string[0], count);

        IEnumerable<Commit> GetCommits(Branch branch, string? sha, string[] commitMessagesToBeIgnored, int count = 500)
        {
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