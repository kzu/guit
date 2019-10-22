using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Plugin.CherryPicker.Properties;
using LibGit2Sharp;

namespace Guit.Plugin.CherryPicker
{
    [Shared]
    [MenuCommand("CherryPicker.SelectBaseBranch", 's', nameof(CherryPicker), typeof(Resources), DefaultVisible = false, IsDynamic = true)]
    class SelectBaseBranchCommand : IDynamicMenuCommand
    {
        readonly CherryPickConfig? config;
        readonly MainThread mainThread;
        readonly CherryPickerView view;

        [ImportingConstructor]
        public SelectBaseBranchCommand(IEnumerable<CherryPickConfig> repositories, IRepository root, MainThread mainThread, CherryPickerView view)
        {
            this.mainThread = mainThread;
            this.view = view;
            IsVisible = IsEnabled = repositories.Count() == 1 && repositories.ElementAt(0).Repository == root;

            if (IsEnabled)
                config = repositories.FirstOrDefault();
        }

        public bool IsVisible { get; }

        public bool IsEnabled { get; }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            if (config != null)
            {
                var dialog = new InputBox(
                    "Select Branch",
                    "Cherry pick from:",
                    config.Repository.Branches.Select(x => x.FriendlyName).ToArray())
                {
                    Text = config.BaseBranch ?? string.Empty
                };

                if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
                {
                    config.BaseBranch = dialog.Text;
                    view.Refresh(false);
                }
            }

            return Task.CompletedTask;
        }
    }
}