using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;

namespace Guit.Plugin.CherryPicker
{
    [Shared]
    [Export]
    [ContentView(nameof(CherryPicker), '4')]
    class CherryPickerView : ContentView
    {
        const string DefaultTitle = "Cherry-Picker";

        readonly IEnumerable<CherryPickConfig> repositories;
        readonly CredentialsHandler credentials;
        readonly MainThread mainThread;
        readonly ICommandService commandService;
        readonly ListView<CommitEntry> listView;

        [ImportingConstructor]
        public CherryPickerView(
            IEnumerable<CherryPickConfig> repositories,
            CredentialsHandler credentials,
            MainThread mainThread,
            ICommandService commandService)
            : base(DefaultTitle)
        {
            this.repositories = repositories;
            this.credentials = credentials;
            this.mainThread = mainThread;
            this.commandService = commandService;

            // TODO: avoid showing the repository.Name column when cherry picking in "root" mode
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
                if (baseBranch is null)
                {
                    commandService.RunAsync("CherryPicker.SelectBaseBranch");
                }
                else
                {
                    var targetBranch = repository.SwitchToTargetBranch(config);

                    var baseCommits = GetCommits(baseBranch, config).ToList();
                    var targetCommits = GetCommits(targetBranch, config, config.Limit * 2).ToList();

                    var missingCommits = baseCommits
                        .Where(baseCommit => !targetCommits.Any(releaseCommit =>
                            string.Compare(baseCommit.Message, releaseCommit.Message, CultureInfo.CurrentCulture, System.Globalization.CompareOptions.IgnoreSymbols) == 0));

                    return missingCommits.Select(x => new CommitEntry(config, x));
                }


                return Enumerable.Empty<CommitEntry>();
            }).ToList());
        }

        IEnumerable<Commit> GetCommits(Branch? branch, CherryPickConfig config, int? limit = default) =>
            branch?
                .Commits
                .Where(commit => config?.IgnoreCommits.Any(ignore => commit.MessageShort.StartsWith(ignore) || commit.Sha == ignore) == false)
                .Take(limit ?? config.Limit) ?? Enumerable.Empty<Commit>();
    }
}