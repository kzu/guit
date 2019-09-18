using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    class PullDialog : DialogBox
    {
        public PullDialog(string remote, string branchName, bool isFastForward)
            : base("Pull")
        {
            Remote = remote;
            Branch = branchName;
            IsFastForward = isFastForward;
        }

        public string Remote { get; set; }

        public string Branch { get; set; }

        public bool IsFastForward { get; set; }

        protected override void EndInit()
        {
            Width = 60;
            Height = 15;

            Add(new StackPanel(
                new Label("Remote"),
                Bind(new TextField(string.Empty) { Height = 1 }, nameof(Remote)),
                new EmptyLine(),
                new Label("Remote Branch"),
                Bind(new TextField(string.Empty) { Height = 1 }, nameof(Branch)),
                new EmptyLine(),
                Bind(new CheckBox("Fast Forward"), nameof(IsFastForward)))
            {
                Y = 1,
                X = 1,
                Width = Dim.Fill(2)
            });

            base.EndInit();
        }
    }
}