using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Log
{
    [Shared]
    [Export]
    [ContentView(nameof(Log), '3')]
    public class LogView : ContentView
    {
        readonly ListViewItemSelector<Commit>[] commitSelectors = new ListViewItemSelector<Commit>[]
            {
                new ListViewItemSelector<Commit>(x => x.MessageShort, "*"),
                new ListViewItemSelector<Commit>(x => x.Author.Name, 15),
                new ListViewItemSelector<Commit>(x => x.Committer.When.ToString("g"), 19)
            };

        List<Commit> commits = new List<Commit>();

        readonly IRepository repository;
        readonly ListView view;

        [ImportingConstructor]
        public LogView(IRepository repository)
            : base("Log")
        {
            this.repository = repository;

            view = new ListView(new List<ListViewItem<Commit>>())
            {
                AllowsMarking = true
            };

            Content = view;
        }

        public Commit? SelectedCommit =>
            view.SelectedItem >= 0 && view.SelectedItem < commits.Count ?
                commits[view.SelectedItem] : null;

        public override void Refresh()
        {
            base.Refresh();

            commits = repository.Commits
                .QueryBy(new CommitFilter())
                .Take(100)
                .ToList();

            if (Frame.Width > 0)
                RefreshCommits();
        }

        protected override void EndInit()
        {
            base.EndInit();

            //RefreshCommits();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            RefreshCommits();
        }

        void RefreshCommits() =>
            view.SetSource(commits.Select(x => new ListViewItem<Commit>(x, Frame.Width - 10, commitSelectors.ToArray())).ToList());
    }
}