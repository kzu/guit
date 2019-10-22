using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    public class ResolveConflictsDialog : DialogBox
    {
        IRepository repository;
        ListView view;
        private ICommandService commands;

        public ResolveConflictsDialog(IRepository repository, ICommandService commands) : base("Resolve Conflicts")
        {
            Buttons = DialogBoxButton.None;

            this.repository = repository;
            this.commands = commands;

            view = new ListView(new List<Conflict>())
            {
                //AllowsMarking = true
            };

            Add(view);
            AddButton("Close", () => Close(true), true);
        }

        protected override void EndInit()
        {
            view.Height = Dim.Fill(2);
            base.EndInit();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            RefreshConflicts();
        }

        private void RefreshConflicts() => view.SetSource(repository.Index.Conflicts.Select(x =>
            new ListViewItem<Conflict>(x, Frame.Width - 4, new ColumnDefinition<Conflict>(c => c.Ours.Path, "*"))).ToList());


        public override void WillPresent()
        {
            base.WillPresent();
            if (repository.Index.Count != 0)
            {
                // Force focus to go to the first entry. Tried everything 
                // else without success:
                // Focus(view)
                // SetNeedsDisplay() (and (view))
                // view.SelectedItem = 0 && ^
                ProcessKey(new KeyEvent(Key.Tab));
            }
        }

        public override bool ProcessKey(KeyEvent kb)
        {
            if (kb.KeyValue == (int)Key.Enter && MostFocused == view)
            {
                // We always handle Enter, either on the current entry or 
                // via Ok/Cancel buttons on the base dialog, which is why 
                // we will always return true.
                ShowMerge();
                return true;
            }

            return base.ProcessKey(kb);
        }

        private void ShowMerge()
        {
            var conflict = repository.Index.Conflicts.Skip(view.SelectedItem).FirstOrDefault();
            if (conflict != null)
                commands.RunAsync(WellKnownCommands.Open, conflict).ContinueWith(_ => RefreshConflicts(), TaskContinuationOptions.OnlyOnRanToCompletion);
        }
    }
}