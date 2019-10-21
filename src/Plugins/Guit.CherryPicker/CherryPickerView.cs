using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
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
        readonly ICommandService commandService;
        readonly IHistoryDivergenceService historyDivergenceService;
        readonly ListView<CommitEntry> listView;

        [ImportingConstructor]
        public CherryPickerView(IEnumerable<CherryPickConfig> repositories, ICommandService commandService, IHistoryDivergenceService historyDivergenceService)
            : base(DefaultTitle)
        {
            this.repositories = repositories;
            this.commandService = commandService;
            this.historyDivergenceService = historyDivergenceService;

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
                if (config.Repository.GetBaseBranch(config) is Branch baseBranch)
                {
                    var targetBranch = config.Repository.SwitchToTargetBranch(config);

                    if (historyDivergenceService.TryGetDivergence(config.Repository, baseBranch, targetBranch, out var missingCommits, true))
                    {
                        // Filter ignores
                        missingCommits = missingCommits.Where(x =>
                            !config.IgnoreCommits.Any(ignore => x.MessageShort.StartsWith(ignore) || ignore.Contains(x.Sha)));

                        return missingCommits.Select(x => new CommitEntry(config, x));
                    }

                    return Enumerable.Empty<CommitEntry>();
                }
                else
                {
                    commandService.RunAsync("CherryPicker.SelectBaseBranch");

                    return Enumerable.Empty<CommitEntry>();
                }
            }).ToList());
        }
    }
}