using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit.Plugin.Releaseator
{
    class PushDialog : DialogBox
    {
        readonly (string Repo, string Branch, TextField TextField)[] repositories;

        public PushDialog(params (string Repo, string Branch)[] repositories)
            : base("Push")
        {
            this.repositories = repositories.Select(x => (x.Repo, x.Branch, new TextField(x.Branch))).ToArray();
        }

        protected override void EndInit()
        {
            var repoLabelWidth = repositories.Max(x => x.Repo.Length) + 2;
            var branchTextFieldWidth = repositories.Max(x => x.Branch.Length) + 20;

            Width = 8 + repoLabelWidth + branchTextFieldWidth;
            Height = 10 + repositories.Length;

            var panel = new StackPanel() { X = 1, Y = 1, Width = Dim.Fill(1) };
            panel.Add(
                repositories
                    .Select(x => new StackPanel(StackPanelOrientation.Horizontal,
                        new Label(x.Repo + ":") { Width = repoLabelWidth },
                        GetTextField(x.Repo, branchTextFieldWidth))).ToArray());

            Add(panel);

            base.EndInit();
        }

        TextField GetTextField(string repo, int width = default)
        {
            var textField = repositories.Where(x => x.Repo == repo).Select(x => x.TextField).Single();

            if (width != default)
                textField.Width = width;

            return textField;
        }

        public IEnumerable<(string Repo, string BranchName)> Branches =>
            repositories.Select(x => (x.Repo, x.TextField.Text.ToString()));
    }
}