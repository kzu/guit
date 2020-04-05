using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    class CommitDialog : DialogBox
    {
        TextView textView = new TextView();
        string? message;

        public CommitDialog(string title, string? message = null) : base(title)
        {
            this.message = message;
        }

        protected override void EndInit()
        {
            Height = Dim.Fill(5);

            AddButton("Commit To", OnNewBranchClicked);

            textView.X = 1;
            textView.Y = 1;
            textView.Width = Dim.Fill(1);
            textView.Height = Dim.Height(this) - 7;
            textView.Text = message ?? string.Empty;

            Add(textView);

            // Set IsInitialized and raise Initialized at the end.
            base.EndInit();
        }

        public string? Message
        {
            get => IsInitialized ? textView.Text.ToString() : message;
            set => message = value;
        }

        public string? NewBranchName { get; set; }

        void OnNewBranchClicked()
        {
            var dialog = new InputBox("New Branch Name", "Commit to:");

            if (dialog.ShowDialog() == true)
            {
                NewBranchName = dialog.Text;
                Close(true);
            }
        }
    }
}