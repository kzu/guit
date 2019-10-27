using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Guit.Plugin.CherryPicker
{
    [Shared]
    [CherryPickerCommand(WellKnownCommands.CherryPicker.SelectBaseBranch, 's', DefaultVisible = false, IsDynamic = true)]
    class SelectBaseBranchCommand : IDynamicMenuCommand
    {
        readonly CherryPickConfig? config;
        readonly MainThread mainThread;
        readonly CherryPickerView view;

        [ImportingConstructor]
        public SelectBaseBranchCommand(IEnumerable<CherryPickConfig> repositories, MainThread mainThread, CherryPickerView view)
        {
            this.mainThread = mainThread;
            this.view = view;

            IsVisible = IsEnabled = view.IsRootMode;

            if (IsEnabled)
                config = repositories.FirstOrDefault();
        }

        public bool IsVisible { get; }

        public bool IsEnabled { get; }

        public Task ExecuteAsync(CancellationToken cancellation = default)
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