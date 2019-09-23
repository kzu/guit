using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    class CommitDialog : DialogBox
    {
        TextView textView;

        public CommitDialog() : base("Commit") { }

        protected override void EndInit()
        {
            Height = Dim.Fill(5);

            AddButton("Commit To", OnNewBranchClicked);

            textView = new TextView()
            {
                X = 1,
                Y = 1,
                Width = Dim.Fill(1),
                Height = Dim.Height(this) - 7,
                Text = string.Empty
            };

            InitialFocusedView = textView;

            Add(textView);

            // Set IsInitialized and raise Initialized at the end.
            base.EndInit();
        }

        public string Message => textView.Text.ToString();

        public string NewBranchName { get; private set; }

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