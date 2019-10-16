using System;
using System.Linq;
using System.Collections.Generic;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    class FetchAndRebaseDialog : DialogBox
    {
        readonly ListViewItemSelector<Commit>[] commitSelectors = new ListViewItemSelector<Commit>[]
            {
                new ListViewItemSelector<Commit>(x => x.MessageShort, "*"),
                new ListViewItemSelector<Commit>(x => x.Author.Name, 15),
                new ListViewItemSelector<Commit>(x => x.Committer.When.ToString("g"), 19)
            };

        readonly IEnumerable<Commit> commits;
        readonly ListView view;

        public FetchAndRebaseDialog(IEnumerable<Commit> commits) : base("Fetch & Rebase")
        {
            this.commits = commits;

            view = new ListView(new List<ListViewItem<Commit>>())
            {
                AllowsMarking = true,
            };
        }

        protected override void EndInit()
        {
            base.EndInit();

            Width = Dim.Fill(4);
            Height = Dim.Fill(4);

            view.Y = view.X = 1;
            view.Width = Dim.Fill(1);
            view.Height = Dim.Fill(2);

            Add(view);
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            view.SetSource(commits.Select(x => new ListViewItem<Commit>(x, Frame.Width - 10, commitSelectors.ToArray())).ToList());
        }
    }
}