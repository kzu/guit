using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    class CommitDialog : DialogBox
    {
        public CommitDialog() : base("Commit") { }

        protected override void EndInit()
        {
            Height = Dim.Fill(5);

            InitialFocusedView = Add(
                    new TextView()
                    {
                        X = 1,
                        Y = 1,
                        Width = Dim.Fill(1),
                        Height = Dim.Height(this) - 7
                    }, nameof(Message));

            AddButton("Commit To", OnNewBranchClicked);

            // Set IsInitialized and raise Initialized at the end.
            base.EndInit();
        }

        public string Message { get; set; }

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