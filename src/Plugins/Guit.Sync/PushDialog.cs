using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    class PushDialog : DialogBox
    {
        public PushDialog(string remote, string branchName, bool force = false)
            : base("Push")
        {
            Remote = remote;
            Branch = branchName;
            Force = force;
        }

        public string Remote { get; set; }

        public string Branch { get; set; }

        public bool Force { get; set; }

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
                Bind(new CheckBox("Force"), nameof(Force)))
            {
                Y = 1,
                X = 1,
                Width = Dim.Fill(2)
            });

            base.EndInit();
        }
    }
}