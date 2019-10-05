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

        List<CommitEntry> commits = new List<CommitEntry>();
        readonly ListView view;

        [ImportingConstructor]
        public LogView(IRepository repository)
            : base("Log")
        {
            this.repository = repository;

            view = new ListView(commits)
            {
                AllowsMarking = true
            };

            Content = view;
        }

        public Commit? SelectedCommit =>
            view.SelectedItem >= 0 && view.SelectedItem < commits.Count ?
                commits[view.SelectedItem].Commit : null;

        public override void Refresh()
        {
            base.Refresh();

            commits = repository.Commits
                .QueryBy(new CommitFilter())
                .Select(x => new CommitEntry(x))
                .Take(50)
                .ToList();

            view.SetSource(commits);
        }

        class CommitEntry
        {
            public CommitEntry(Commit commit)
            {
                Commit = commit;
            }

            public Commit Commit { get; }

            public override string ToString()
            {
                return GetNormalizedString(Commit.MessageShort, 70) +
                    GetNormalizedString(Commit.Author.Name, 25) +
                    Commit.Committer.When;
            }

            string GetNormalizedString(string value, int length)
            {
                if (value.Length < length)
                    return string.Concat(value, new String(' ', length - value.Length));

                return value;
            }
        }
    }
}