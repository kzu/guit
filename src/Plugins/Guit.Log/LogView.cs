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
        readonly IRepository repository;
        readonly ListView<Commit> view;

        [ImportingConstructor]
        public LogView(IRepository repository)
            : base("Log")
        {
            this.repository = repository;

            view = new ListView<Commit>(
                    new ColumnDefinition<Commit>(x => x.MessageShort, "*"),
                    new ColumnDefinition<Commit>(x => x.Author.Name, 15),
                    new ColumnDefinition<Commit>(x => x.Committer.When.ToString("g"), 19))
            {
                AllowsMarking = true
            };

            Content = view;
        }

        public Commit? SelectedCommit =>
            view.SelectedItem >= 0 && view.SelectedItem < view.Values.Count() ?
                view.Values.ElementAt(view.SelectedItem) : null;

        public override void Refresh()
        {
            base.Refresh();

            var commits = repository.Commits
                .QueryBy(new CommitFilter())
                .Take(100)
                .ToList();

            view.SetValues(commits);
        }
    }
}