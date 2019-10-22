using System.Collections.Generic;
using System.Composition;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;

namespace Guit.Plugin.CherryPicker
{
    [Shared]
    [CherryPickerCommand(WellKnownCommands.CherryPicker.Ignore, 'i')]
    class IgnoreCommand : IMenuCommand
    {
        readonly MainThread mainThread;
        readonly CherryPickerView view;

        [ImportingConstructor]
        public IgnoreCommand(MainThread mainThread, CherryPickerView view)
        {
            this.mainThread = mainThread;
            this.view = view;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var entries = view.MarkedEntries.ToList();

            if (entries.Any())
            {
                var dialog = new MessageBox(
                    string.Format("Ignore Commits"),
                    DialogBoxButton.Ok | DialogBoxButton.Cancel,
                    string.Format("We will ignore and add the selected {0} commit(s) into the {1} file", entries.Count(), Constants.NoCherryPick));

                if (mainThread.Invoke(() => dialog.ShowDialog()) == true)
                {
                    // Create the file if it does not exist
                    var ignoreCommitsLines = File.Exists(Constants.NoCherryPick) ?
                         File.ReadAllLines(Constants.NoCherryPick).ToList() : new List<string>();

                    // Iterate marked entries by repository
                    foreach (var selectedEntry in entries.GroupBy(x => x.Config))
                    {
                        // This is how it looks the ignore in the config file
                        //
                        // [repository "$(RepoName)"]
                        //     ignore = $(CommitUrl)
                        //
                        var repoSection = $"[repository \"{selectedEntry.Key.Repository.GetName()}\"]";
                        var ignores = selectedEntry.Select(commitEntry =>
                            $"\tignore = {selectedEntry.Key.Repository.GetRepoUrl() + $"/commit/{commitEntry.Commit.Sha}"}");

                        // Search if the section already exists in the file
                        var existingRepoSection = ignoreCommitsLines.FirstOrDefault(x =>
                            string.Compare(x, repoSection, CultureInfo.CurrentCulture, System.Globalization.CompareOptions.IgnoreSymbols) == 0);

                        if (string.IsNullOrEmpty(existingRepoSection))
                            ignoreCommitsLines.Add(repoSection);
                        else
                            repoSection = existingRepoSection;

                        // Insert the new ignores
                        ignoreCommitsLines.InsertRange(ignoreCommitsLines.IndexOf(repoSection) + 1, ignores);

                        // Update the CherryPickConfig
                        selectedEntry.Key.IgnoreCommits = selectedEntry.Key.IgnoreCommits.Concat(ignores);
                    }

                    // And write the ignored commits
                    File.WriteAllLines(Constants.NoCherryPick, ignoreCommitsLines);

                    mainThread.Invoke(() => view.Refresh());
                }
            }

            return Task.CompletedTask;
        }
    }
}