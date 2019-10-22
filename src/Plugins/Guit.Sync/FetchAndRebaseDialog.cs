using System;
using System.Linq;
using System.Collections.Generic;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    class FetchAndRebaseDialog : DialogBox
    {
        readonly ListView<Commit> view;

        public FetchAndRebaseDialog(IEnumerable<Commit> commits) : base("Fetch & Rebase")
        {
            view = new ListView<Commit>(
                new ColumnDefinition<Commit>(x => x.MessageShort, "*"),
                new ColumnDefinition<Commit>(x => x.Author.Name, 15),
                new ColumnDefinition<Commit>(x => x.Committer.When.ToString("g"), 19))
            {
                AllowsMarking = true,
            };

            view.SetValues(commits);
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
    }
}