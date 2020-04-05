using System;
using System.Linq;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Merq;
using Guit.Events;

namespace Guit.Plugin.Sync
{
    [Shared]
    [SyncCommand(WellKnownCommands.Sync.SwitchCheckout, 's')]
    public class SwitchCommand : IMenuCommand
    {
        readonly MainThread mainThread;
        readonly IGitRepository repository;
        readonly IEventStream eventStream;

        [ImportingConstructor]
        public SwitchCommand(MainThread mainThread, IGitRepository repository, IEventStream eventStream)
        {
            this.mainThread = mainThread;
            this.repository = repository;
            this.eventStream = eventStream;
        }

        public Task ExecuteAsync(CancellationToken cancellation = default)
        {
            var dialog = new SwitchDialog(branches: repository.Branches.Select(x => x.FriendlyName).OrderBy(x => x).ToArray());

            if (mainThread.Invoke(() => dialog.ShowDialog()) == true && !string.IsNullOrEmpty(dialog.Branch))
            {
                var branch = repository.Branches.FirstOrDefault(x => x.FriendlyName == dialog.Branch);

                var targetBranch = branch;
                var targetBranchName = dialog.Branch;
                var overwriteTargetBranch = false;

                // Check if the selected branch is remote
                if (branch?.IsRemote == true)
                {
                    // Get the branch name from the remote
                    targetBranchName = branch.GetName();

                    // Allow the user to create a branch for the remote
                    var createBranchDialog = new InputBox("Create Branch", "Branch") { Text = targetBranchName };
                    if (mainThread.Invoke(() => createBranchDialog.ShowDialog()) == true)
                    {
                        // Check if the new branch already exists
                        targetBranchName = createBranchDialog.Text;
                        targetBranch = repository.Branches.FirstOrDefault(x =>
                            !x.IsRemote && x.FriendlyName == createBranchDialog.Text);

                        if (targetBranch != null)
                        {
                            // The branch already exist => ask the user to overwrite it
                            var forceDialog = new MessageBox(
                                "Warning",
                                DialogBoxButton.Ok | DialogBoxButton.Cancel,
                                "A branch with this name already exists. Do you want to overwrite it?");

                            overwriteTargetBranch = mainThread.Invoke(() => forceDialog.ShowDialog() == true);
                            if (!overwriteTargetBranch)
                                return CancelCheckout();
                        }
                    }
                    else return CancelCheckout();
                }

                // 1. Check the remote branch if remote was selected
                if (branch?.IsRemote == true)
                    repository.Checkout(branch);

                // 2. Remove the existing branch if the user decided to overwrite it
                if (overwriteTargetBranch && targetBranch != null)
                {
                    eventStream.Push(Status.Create(0.2f, "Removing branch {0}", targetBranch.FriendlyName));
                    repository.Branches.Remove(targetBranch);
                }

                // 3. Create the branch if it does not exist
                if (targetBranch == null)
                {
                    eventStream.Push(Status.Create(0.4f, "Creating branch {0}", targetBranchName));
                    targetBranch = repository.CreateBranch(targetBranchName);
                }

                // 4. Checkout the branch
                eventStream.Push(Status.Create(0.6f, "Swithing to branch {0}", targetBranchName));
                repository.Checkout(targetBranch);

                // 5. Update submodules
                if (dialog.UpdateSubmodules)
                {
                    eventStream.Push(Status.Create(0.8f, "Updating submodules..."));
                    repository.UpdateSubmodules(eventStream: eventStream);
                }

                eventStream.Push(new BranchChanged(targetBranchName));
                eventStream.Push(Status.Succeeded());
            }

            return Task.CompletedTask;
        }

        public Task CancelCheckout()
        {
            eventStream.Push(Status.Failed());
            return Task.CompletedTask;
        }
    }
}