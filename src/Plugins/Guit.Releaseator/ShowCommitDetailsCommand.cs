using System;
using System.Composition;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    [MenuCommand("Details", Terminal.Gui.Key.F3, nameof(Releaseator), Visible = false)]
    class ShowCommitDetailsCommand : IMenuCommand
    {
        const string GitSuffix = ".git";

        readonly ReleaseatorView view;

        [ImportingConstructor]
        public ShowCommitDetailsCommand(ReleaseatorView view) => this.view = view;

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            if (view.SelectedEntry is CommitEntry selectedEntry)
            {
                var repoUrl = selectedEntry.Repository.Config.GetValueOrDefault<string>("remote.origin.url");
                if (repoUrl.EndsWith(GitSuffix))
                    repoUrl = repoUrl.Remove(repoUrl.Length - GitSuffix.Length);

                Process.Start("cmd", $"/c start {repoUrl}/commit/{selectedEntry.Commit.Sha}");
            }

            return Task.CompletedTask;
        }
    }
}