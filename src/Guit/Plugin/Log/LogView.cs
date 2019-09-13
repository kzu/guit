using System;
using System.Linq;
using System.Collections.Generic;
using System.Composition;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Log
{
    [Shared]
    [Export]
    public class LogView : ContentView
    {
        readonly Repository repository;

        List<CommitInfo> commits = new List<CommitInfo>();
        readonly ListView view;

        [ImportingConstructor]
        public LogView(Repository repository)
            : base("Log")
        {
            this.repository = repository;

            view = new ListView(commits)
            {
                AllowsMarking = true
            };

            Content = view;
        }

        public override void Refresh()
        {
            base.Refresh();

            commits = repository.Commits
                .QueryBy(new CommitFilter())
                .Select(x =>
                    new CommitInfo
                    {
                        Message = x.MessageShort,
                        Author = x.Author.Name,
                        When = x.Committer.When
                    })
                .Take(50)
                .ToList();

            view.SetSource(commits);
        }

        public override string Context => nameof(Log);

        class CommitInfo
        {
            public string Message { get; set; }

            public string Author { get; set; }

            public DateTimeOffset When { get; set; }

            public override string ToString()
            {
                return GetNormalizedString(Message, 70) +
                    GetNormalizedString(Author, 25) +
                    When;
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