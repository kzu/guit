using System;
using System.Linq;
using System.Collections.Generic;
using System.Composition;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    [Export]
    [ContentView(nameof(Releaseator), '4')]
    class ReleaseatorView : ContentView
    {
        readonly ListViewItemSelector<CommitEntry>[] commitSelectors = new ListViewItemSelector<CommitEntry>[]
            {
                new ListViewItemSelector<CommitEntry>(x => x.Repository.GetName(), 30),
                new ListViewItemSelector<CommitEntry>(x => x.Commit.MessageShort, "*"),
                new ListViewItemSelector<CommitEntry>(x => x.Commit.Author.Name, 15),
                new ListViewItemSelector<CommitEntry>(x => x.Commit.Committer.When.ToString("g"), 19)
            };

        List<CommitEntry> commits = new List<CommitEntry>();

        readonly IEnumerable<IRepository> repositories;
        readonly RepositoryConfigProvider repositoryConfigProvider;
        readonly ListView view;

        [ImportingConstructor]
        public ReleaseatorView(IEnumerable<IRepository> repositories, RepositoryConfigProvider repositoryConfigProvider)
            : base(nameof(Releaseator))
        {
            this.repositories = repositories;
            this.repositoryConfigProvider = repositoryConfigProvider;

            view = new ListView(new List<ListViewItem<CommitEntry>>())
            {
                AllowsMarking = true
            };

            Content = view;
        }
        public IEnumerable<CommitEntry> MarkedEntries =>
            commits
                .Where((x, i) => view.Source.IsMarked(i))
                .Select(x => x);

        public CommitEntry? SelectedEntry =>
            view.SelectedItem >= 0 && view.SelectedItem < commits.Count ?
                commits[view.SelectedItem] : null;

        public override void Refresh()
        {
            base.Refresh();

            commits.Clear();

            foreach (var repository in repositories)
            {
                var config = repositoryConfigProvider.GetConfig(repository);

                if (config != null)
                {
                    var baseCommits = GetCommits(repository, config.BaseBranch, config.BaseBranchSha, config.IgnoreCommits).ToList();
                    var targetCommits = GetCommits(repository, config.TargetBranch, config.TargetBranchSha, config.IgnoreCommits).ToList();

                    var missingCommits = baseCommits
                        .Where(baseCommit => !targetCommits.Any(targetCommit => baseCommit.MessageShort == targetCommit.MessageShort));

                    commits.AddRange(missingCommits.Select(x => new CommitEntry(repository, x)));
                }
            }

            if (Frame.Width > 0)
                RefreshCommits();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            RefreshCommits();
        }

        void RefreshCommits()
        {
            view.SetSource(
                commits
                    .Select(x => new ListViewItem<CommitEntry>(x, Frame.Width - 10, commitSelectors.ToArray()))
                    .ToList());
        }

        IEnumerable<Commit> GetCommits(IRepository repository, string branchName, string? sha, string[]? commitMessagesToBeIgnored) =>
            GetCommits(repository.Branches.Single(x => x.FriendlyName == branchName), sha, commitMessagesToBeIgnored ?? new string[0]);

        IEnumerable<Commit> GetCommits(Branch branch, string? sha, string[] commitMessagesToBeIgnored)
        {
            var count = 0;

            foreach (var commit in branch.Commits)
            {
                if (!commitMessagesToBeIgnored.Any(x => commit.MessageShort.StartsWith(x)))
                {
                    count++;
                    yield return commit;
                }

                if (commit.Sha == sha || count >= 200)
                    break;
            }
        }
    }
}