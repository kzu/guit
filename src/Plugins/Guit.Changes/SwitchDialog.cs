using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    class SwitchDialog : DialogBox
    {
        readonly IEnumerable<string> branches;

        public SwitchDialog(bool updateSubmodules = true, IEnumerable<string>? branches = null)
            : base("Switch/Checkout")
        {
            UpdateSubmodules = updateSubmodules;

            this.branches = branches ?? Enumerable.Empty<string>();
        }

        public string Branch { get; set; } = string.Empty;

        public bool UpdateSubmodules { get; set; }

        protected override void EndInit()
        {
            Width = 60;
            Height = 12;

            Add(new StackPanel(
                new Label("Branch"),
                Bind(new CompletionTextField(branches.ToArray()) { Height = 1 }, nameof(Branch)),
                new EmptyLine(),
                Bind(new CheckBox("Update Submodules"), nameof(UpdateSubmodules)))
            {
                Y = 1,
                X = 1,
                Width = Dim.Fill(2)
            });

            base.EndInit();
        }
    }
}