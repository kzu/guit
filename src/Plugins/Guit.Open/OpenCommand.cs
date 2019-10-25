using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Configuration;
using Guit.Plugin.Changes.Properties;
using LibGit2Sharp;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand(WellKnownCommands.Open, 'o', nameof(WellKnownViews.Changes), resources: typeof(Resources))]
    public class OpenCommand : IMenuCommand
    {
        readonly IRepository repository;
        readonly string editorTool;
        readonly DiffTool diffTool;
        readonly MergeTool mergeTool;
        readonly MainThread mainThread;

        [ImportingConstructor]
        public OpenCommand(IRepository repository, [Import("core.editor")] string editorTool, DiffTool diffTool, MergeTool mergeTool, MainThread mainThread)
        {
            this.repository = repository;
            this.editorTool = editorTool;
            this.diffTool = diffTool;
            this.mergeTool = mergeTool;
            this.mainThread = mainThread;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            var filePath = parameter as string;
            var conflict = parameter as Conflict;

            if (!string.IsNullOrEmpty(filePath))
                conflict = repository.Index.Conflicts[filePath];

            // Nothing to work on in this case.
            if (filePath == null && conflict == null)
                return Task.CompletedTask;

            if (conflict == null)
#pragma warning disable CS8604 // Possible null reference argument.
                return Open(filePath);
#pragma warning restore CS8604 // Possible null reference argument.
            else
                return Merge(conflict);
        }

        private Task Open(string filePath)
        {
            var status = repository.RetrieveStatus(new StatusOptions { PathSpec = new[] { filePath } })[filePath];
            if (status == null)
                return Task.CompletedTask;

            switch (status.State)
            {
                case FileStatus.NewInIndex:
                case FileStatus.NewInWorkdir:
                case FileStatus.RenamedInIndex:
                case FileStatus.RenamedInWorkdir:
                case FileStatus.Unaltered:
                    RunTool(editorTool, repository.GetFullPath(filePath));
                    break;
                case FileStatus.ModifiedInIndex:
                case FileStatus.ModifiedInWorkdir:
                    Diff(filePath);
                    break;
                default:
                    return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        private Task Diff(string filePath)
        {
            // TODO: throw? warn?
            if (string.IsNullOrEmpty(diffTool.Cmd))
                return Task.CompletedTask;

            var changes = repository.Diff.Compare<TreeChanges>().Modified.First(x => x.Path == filePath);
            var old = repository.Lookup<Blob>(changes.OldOid);

            // We can't show diff in this case
            if (changes.Mode != Mode.NonExecutableFile || !changes.Exists || old == null || old.IsBinary)
                return Task.CompletedTask;

            // Write the old content to a temp file for comparison.
            var baseFile = Path.Combine(Path.GetTempPath(), Path.ChangeExtension(Path.GetFileName(filePath), ".BASE" + Path.GetExtension(filePath)));
            File.WriteAllText(baseFile, old.GetContentText());

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var indexOfDolar = diffTool.Cmd.IndexOf("$");
            if (indexOfDolar == -1)
                // TODO: There were no replacements, so we can't diff anything. Report warning/error?
                return Task.CompletedTask;

            var indexOfArgs = diffTool.Cmd.Substring(0, indexOfDolar).LastIndexOf(' ');
            var toolFilePath = diffTool.Cmd.Substring(0, indexOfArgs).Trim();
            var fullFilePath = repository.GetFullPath(filePath);

            // Replace arguments as documented in https://git-scm.com/docs/git-difftool#_options
            var arguments = diffTool.Cmd.Substring(indexOfArgs)
                // "$LOCAL is set to the name of the temporary file containing the contents of the diff pre-image"
                .Replace("$LOCAL", baseFile)
                // "$REMOTE is set to the name of the temporary file containing the contents of the diff post-image"
                // We want the current file to be the one including the "changes post-image".
                .Replace("$REMOTE", fullFilePath)
                // Yes, it's confusing even in the docs that $BASE == $MERGED. 
                // "$MERGED is the name of the file which is being compared.
                .Replace("$MERGED", fullFilePath)
                // "$BASE is provided for compatibility with custom merge tool commands and has the same value as $MERGED."
                .Replace("$BASE", fullFilePath);

#pragma warning restore CS8602 // Dereference of a possibly null reference.

            RunTool(toolFilePath, arguments);

            return Task.CompletedTask;
        }

        private static void RunTool(string toolFilePath, string arguments)
        {
            var psi = new ProcessStartInfo(toolFilePath, arguments);
            if (toolFilePath.Trim('"').EndsWith("code"))
            {
                psi.UseShellExecute = true;
                psi.CreateNoWindow = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
            }
            Process.Start(psi);
        }

        private Task Merge(Conflict conflict)
        {
            // TODO: throw? warn?
            if (string.IsNullOrEmpty(mergeTool.Cmd))
                return Task.CompletedTask;

            var baseFile = repository.GetFullPath(Path.ChangeExtension(conflict.Ancestor.Path, ".BASE" + Path.GetExtension(conflict.Ancestor.Path)));
            File.WriteAllText(baseFile, repository.Lookup<Blob>(conflict.Ancestor.Id).GetContentText());

            var oursFile = repository.GetFullPath(Path.ChangeExtension(conflict.Ours.Path, ".MINE" + Path.GetExtension(conflict.Ours.Path)));
            // We assume the local file already has the content we want, since we Stash before Merge
            File.WriteAllText(oursFile, repository.Lookup<Blob>(conflict.Ours.Id).GetContentText());

            var theirsFile = repository.GetFullPath(Path.ChangeExtension(conflict.Theirs.Path, ".THEIRS" + Path.GetExtension(conflict.Theirs.Path)));
            File.WriteAllText(theirsFile, repository.Lookup<Blob>(conflict.Theirs.Id).GetContentText());

            var mergedFile = repository.GetFullPath(conflict.Ours.Path);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            var indexOfDolar = mergeTool.Cmd.IndexOf("$");
            if (indexOfDolar == -1)
                // TODO: There were no replacements, so we can't merge anything. Report warning/error?
                return Task.CompletedTask;

            var indexOfArgs = mergeTool.Cmd.Substring(0, indexOfDolar).LastIndexOf(' ');
            var toolFilePath = mergeTool.Cmd.Substring(0, indexOfArgs).Trim();

            // Replace arguments as documented in https://git-scm.com/docs/git-mergetool#_options
            var arguments = mergeTool.Cmd.Substring(indexOfArgs)
                .Replace("$LOCAL", oursFile)
                .Replace("$REMOTE", theirsFile)
                .Replace("$BASE", baseFile)
                .Replace("$MERGED", mergedFile);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            try
            {
                var process = Process.Start(toolFilePath, arguments);
                process.WaitForExit();
                if (mergeTool.TrustExitCode && process.ExitCode == 0)
                {
                    // Stage the resolved conflict.
                    repository.Index.Add(conflict.Ours.Path);
                    repository.Index.Write();
                }

                return Task.CompletedTask;
            }
            finally
            {
                if (!mergeTool.KeepBackup)
                {
                    File.Delete(baseFile);
                    File.Delete(oursFile);
                    File.Delete(theirsFile);
                }
            }
        }
    }
}