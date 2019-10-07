using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    class PushDialog : DialogBox
    {
        readonly IEnumerable<string> remotes;
        readonly IEnumerable<string> branches;

        public PushDialog(
            string remote,
            string branchName,
            bool force = false,
            bool trackRemoteBranch = false,
            IEnumerable<string>? remotes = null,
            IEnumerable<string>? branches = null)
            : base("Push")
        {
            Remote = remote;
            Branch = branchName;
            TrackRemoteBranch = trackRemoteBranch;
            Force = force;

            this.remotes = remotes ?? Enumerable.Empty<string>();
            this.branches = branches ?? Enumerable.Empty<string>();
        }

        public string Remote { get; set; }

        public string Branch { get; set; }

        public bool Force { get; set; }

        public bool TrackRemoteBranch { get; set; }

        protected override void EndInit()
        {
            Width = 60;
            Height = 15;

            Add(new StackPanel(
                new Label("Remote"),
                Bind(new CompletionTextField(remotes.ToArray()) { Height = 1 }, nameof(Remote)),
                new EmptyLine(),
                new Label("Remote Branch"),
                Bind(new CompletionTextField(branches.ToArray()) { Height = 1 }, nameof(Branch)),
                new EmptyLine(),
                Bind(new CheckBox("Track Remote Branch"), nameof(TrackRemoteBranch)),
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