using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using LibGit2Sharp;

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
        public CherryPickerView(
            IRepository root,
            IEnumerable<CherryPickConfig> repositories,
            ICommandService commandService,
            IHistoryDivergenceService historyDivergenceService)
            : base(DefaultTitle)
        {
            this.repositories = repositories;
            this.commandService = commandService;
            this.historyDivergenceService = historyDivergenceService;

            IsRootMode = repositories.Count() == 1 && repositories.ElementAt(0).Repository == root;

            listView = new ListView<CommitEntry>(
                IsRootMode ? new NullColumnDefinition<CommitEntry>() : new ColumnDefinition<CommitEntry>(x => x.Config.Repository.GetName(), 30),
                new ColumnDefinition<CommitEntry>(x => x.Commit.GetShortSha(), 10),
                new ColumnDefinition<CommitEntry>(x => x.Commit.MessageShort, "*"),
                new ColumnDefinition<CommitEntry>(x => x.Commit.Author.Name, 15),
                new ColumnDefinition<CommitEntry>(x => x.Commit.Committer.When.ToString("g"), 20))
            {
                AllowsMarking = true
            };

            Content = listView;
        }

        public bool IsRootMode { get; }

        public IEnumerable<CommitEntry> MarkedEntries => listView.MarkedEntries;

        public CommitEntry? SelectedEntry => listView.SelectedEntry;

        public override void Refresh() => Refresh(true);

        public void Refresh(bool selectBaseBranch = true)
        {
            base.Refresh();

            if (repositories.Count() == 1 && repositories.First() is CherryPickConfig config && config.Repository.GetBaseBranch(config) is Branch baseBranch)
                Title = string.Format("{0} (from {1})", DefaultTitle, config.BaseBranch);
            else
                Title = DefaultTitle;

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
                    if (selectBaseBranch)
                        commandService.RunAsync(WellKnownCommands.CherryPicker.SelectBaseBranch);

                    return Enumerable.Empty<CommitEntry>();
                }
            }).ToList());
        }
    }
}