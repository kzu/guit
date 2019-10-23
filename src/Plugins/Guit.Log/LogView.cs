using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using LibGit2Sharp;

namespace Guit.Plugin.Log
{
    [Shared]
    [Export]
    [ContentView(WellKnownViews.Log, '3')]
    class LogView : ContentView
    {
        readonly IRepository repository;
        readonly ListView<CommitEntry> view;

        [ImportingConstructor]
        public LogView(IRepository repository)
            : base("Log")
        {
            this.repository = repository;

            view = new ListView<CommitEntry>(
                    new ColumnDefinition<CommitEntry>(x => x.Commit.GetShortSha(), 10),
                    new ColumnDefinition<CommitEntry>(x => x.Commit.MessageShort, "*"),
                    new ColumnDefinition<CommitEntry>(x => x.Commit.Author.Name, 15),
                    new ColumnDefinition<CommitEntry>(x => x.Commit.Committer.When.ToString("g"), 19))
            {
                X = 1
            };

            Content = view;
        }

        public CommitEntry SelectedEntry => view.SelectedEntry;

        public override void Refresh()
        {
            base.Refresh();

            // TODO: implement virtual list view and provide more commits once the
            // vertical scroll bar is moved down
            var commits = repository.Commits
                .Take(100)
                .Select(x => new CommitEntry(repository, x))
                .ToList();

            view.SetValues(commits);
        }
    }
}