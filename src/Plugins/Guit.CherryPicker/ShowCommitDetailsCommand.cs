using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Guit.Plugin.CherryPicker
{
    [Shared]
    [MenuCommand("Details", Terminal.Gui.Key.F3, nameof(CherryPicker), DefaultVisible = false)]
    class ShowCommitDetailsCommand : IMenuCommand
    {
        readonly CherryPickerView view;

        [ImportingConstructor]
        public ShowCommitDetailsCommand(CherryPickerView view) => this.view = view;

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            if (view.SelectedEntry is CommitEntry selectedEntry)
                Process.Start("cmd", $"/c start {view.SelectedEntry.Config.Repository.GetRepoUrl()}/commit/{selectedEntry.Commit.Sha}");

            return Task.CompletedTask;
        }
    }
}