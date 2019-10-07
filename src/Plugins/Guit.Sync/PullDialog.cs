using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    class PullDialog : DialogBox
    {
        readonly IEnumerable<string> remotes;
        readonly IEnumerable<string> branches;

        public PullDialog(
            string remote,
            string branchName,
            bool isFastForward = true,
            bool trackRemoteBranch = false,
            bool updateSubmodules = true,
            IEnumerable<string>? remotes = null,
            IEnumerable<string>? branches = null)
            : base("Pull")
        {
            Remote = remote;
            Branch = branchName;
            IsFastForward = isFastForward;
            TrackRemoteBranch = trackRemoteBranch;
            UpdateSubmodules = updateSubmodules;

            this.remotes = remotes ?? Enumerable.Empty<string>();
            this.branches = branches ?? Enumerable.Empty<string>();
        }

        public string Remote { get; set; }

        public string Branch { get; set; }

        public bool IsFastForward { get; set; }

        public bool TrackRemoteBranch { get; set; }

        public bool UpdateSubmodules { get; set; }

        protected override void EndInit()
        {
            Width = 60;
            Height = 19;

            Add(new StackPanel(
                new Label("Remote"),
                Bind(new CompletionTextField(remotes.ToArray()) { Height = 1 }, nameof(Remote)),
                new EmptyLine(),
                new Label("Remote Branch"),
                Bind(new CompletionTextField(branches.ToArray()) { Height = 1 }, nameof(Branch)),
                new EmptyLine(),
                Bind(new CheckBox("Fast Forward"), nameof(IsFastForward)),
                new EmptyLine(),
                Bind(new CheckBox("Update Submodules"), nameof(UpdateSubmodules)),
                new EmptyLine(),
                Bind(new CheckBox("Track Remote Branch"), nameof(TrackRemoteBranch)))
            {
                Y = 1,
                X = 1,
                Width = Dim.Fill(2)
            });

            base.EndInit();
        }
    }
}