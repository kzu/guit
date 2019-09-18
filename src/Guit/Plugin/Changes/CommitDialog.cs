using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    class CommitDialog : DialogBox
    {
        public CommitDialog() : base("Commit", useDefaultButtons: true)
        { }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

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
        }

        public string Message { get; set; }

        public string NewBranchName { get; private set; }

        void OnNewBranchClicked()
        {
            var dialog = new SingleTextInputDialog("New Branch Name", "Commit to:");

            if (dialog.ShowDialog() == true)
            {
                NewBranchName = dialog.Text;
                Close(true);
            }
        }
    }
}