using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Guit.Plugin.CherryPicker
{
    [Shared]
    [Export]
    [ContentView(nameof(CherryPicker), '4')]
    class ReleaseatorView : ContentView
    {
        readonly IEnumerable<CherryPickConfig> repositories;
        readonly CredentialsHandler credentials;
        readonly ListView<CommitEntry> listView;

        [ImportingConstructor]
        public ReleaseatorView(IEnumerable<CherryPickConfig> repositories, CredentialsHandler credentials)
            : base(nameof(CherryPicker))
        {
            this.repositories = repositories;
            this.credentials = credentials;

            listView = new ListView<CommitEntry>(
                new ColumnDefinition<CommitEntry>(x => x.Config.Repository.GetName(), 30),
                new ColumnDefinition<CommitEntry>(x => x.Commit.GetShortSha(), 10),
                new ColumnDefinition<CommitEntry>(x => x.Commit.MessageShort, "*"),
                new ColumnDefinition<CommitEntry>(x => x.Commit.Author.Name, 15),
                new ColumnDefinition<CommitEntry>(x => x.Commit.Committer.When.ToString("g"), 20))
            {
                AllowsMarking = true
            };

            Content = listView;
        }
        public IEnumerable<CommitEntry> MarkedEntries => listView.MarkedEntries;

        public CommitEntry? SelectedEntry => listView.SelectedEntry;

        public override void Refresh()
        {
            base.Refresh();

            listView.SetValues(repositories.SelectMany(config =>
            {
                var repository = config.Repository;

                var baseBranch = repository.GetBaseBranch(config);
                var releaseBranch = repository.SwitchToTargetBranch(config);

                var baseCommits = GetCommits(baseBranch, config).ToList();
                var targetCommits = GetCommits(releaseBranch, config, config.Limit * 2).ToList();

                var missingCommits = baseCommits
                    .Where(baseCommit => !targetCommits.Any(releaseCommit =>
                        string.Compare(baseCommit.Message, releaseCommit.Message, CultureInfo.CurrentCulture, System.Globalization.CompareOptions.IgnoreSymbols) == 0));

                return missingCommits.Select(x => new CommitEntry(config, x));
            }).ToList());
        }

        IEnumerable<Commit> GetCommits(Branch? branch, CherryPickConfig config, int? limit = default) =>
            branch?
                .Commits
                .Where(commit => config?.IgnoreCommits.Any(ignore => commit.MessageShort.StartsWith(ignore) || commit.Sha == ignore) == false)
                .Take(limit ?? config.Limit) ?? Enumerable.Empty<Commit>();
    }
}