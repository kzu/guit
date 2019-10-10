using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    public class ResolveConflictsDialog : DialogBox
    {
        IRepository repository;
        List<Conflict> conflicts;
        ListView view;

        public ResolveConflictsDialog(IRepository repository, IEnumerable<Conflict> conflicts) : base("Resolve Conflicts")
        {
            this.repository = repository;
            this.conflicts = conflicts.ToList();
            view = new ListView(this.conflicts)
            {
                AllowsMarking = true
            };

            Add(view);
        }

        protected override void EndInit()
        {
            view.Height = Dim.Fill(2);
            base.EndInit();
        }

        public override void WillPresent()
        {
            base.WillPresent();
            if (conflicts.Count != 0)
            {
                // Force focus to go to the first entry. Tried everything 
                // else without success:
                // Focus(view)
                // SetNeedsDisplay() (and (view))
                // view.SelectedItem = 0 && ^
                ProcessKey(new KeyEvent(Key.Tab));
            }
        }

        public IEnumerable<Conflict> GetResolvedConflicts() => conflicts.Where((x, i) => view.Source.IsMarked(i));

        public override bool ProcessKey(KeyEvent kb)
        {
            if (kb.KeyValue == (int)Key.Enter && MostFocused == view)
            {
                // We always handle Enter, either on the current entry or 
                // via Ok/Cancel buttons on the base dialog, which is why 
                // we will always return true.
                ShowMerge();
                return true;
            }

            return base.ProcessKey(kb);
        }

        private void ShowMerge()
        {
            var conflict = conflicts[view.SelectedItem];

            string? mergeToolCmd = default;
            var trustExitCode = true;
            var keepBackup = false;

            var mergeToolName = repository.Config.GetValueOrDefault("merge.tool", default(string));
            if (!string.IsNullOrEmpty(mergeToolName))
            {
                // NOTE: we only support cmd-specified tools, since we need to do the replacements of 
                // $BASE $LOCAL $REMOTE and $MERGED ourselves and otherwise wouldn't know the order.
                mergeToolCmd = repository.Config.GetValueOrDefault("mergetool", mergeToolName, "cmd", default(string));
                if (!string.IsNullOrEmpty(mergeToolCmd))
                {
                    trustExitCode = repository.Config.GetValueOrDefault("merge.tool", mergeToolName, "trustExitCode", true);
                    keepBackup = repository.Config.GetValueOrDefault("merge.tool", mergeToolName, "keepBackup", true);
                }
            }

#pragma warning disable CS8602 // Dereference of a possibly null reference. It's not detecting the !string.IsNullOrEmpty :(
            if ((string.IsNullOrEmpty(mergeToolCmd) || mergeToolCmd.StartsWith("vsDiffMerge.exe", StringComparison.OrdinalIgnoreCase)) && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                // Try using VS
                var devenv = Process.GetProcessesByName("devenv").FirstOrDefault();
                var vsDiffMerge = @"CommonExtensions\Microsoft\TeamFoundation\Team Explorer\vsDiffMerge.exe";
                if (devenv != null && File.Exists(Path.Combine(Path.GetDirectoryName(devenv.MainModule.FileName), vsDiffMerge)))
                {
                    vsDiffMerge = Path.Combine(Path.GetDirectoryName(devenv.MainModule.FileName), vsDiffMerge);
                }
                else
                {
                    // If no VS is running, just locate the latest vsDiffMerge we can find
                    var vswhere = Path.Combine(Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location), "vswhere.exe");
                    var process = Process.Start(new ProcessStartInfo(vswhere, $@"-requires Microsoft.VisualStudio.TeamExplorer -latest -find ""Common7\IDE\{vsDiffMerge}""")
                    {
                        RedirectStandardOutput = true
                    });

                    vsDiffMerge = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();
                }

                if (!string.IsNullOrEmpty(vsDiffMerge))
                {
                    // Replace relative-path vsdiffmerge with full path one we located.
                    if (mergeToolCmd?.StartsWith("vsDiffMerge.exe", StringComparison.OrdinalIgnoreCase) == true)
                        mergeToolCmd = vsDiffMerge + " " + mergeToolCmd.Substring(15);
                    else
                        mergeToolCmd = $"{vsDiffMerge} $LOCAL $REMOTE $BASE $MERGED /t /m Mine Theirs";
                }
            }

            if (!string.IsNullOrEmpty(mergeToolCmd))
            {
                var baseFile = Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, Path.ChangeExtension(conflict.Ancestor.Path, ".BASE" + Path.GetExtension(conflict.Ancestor.Path))));
                File.WriteAllText(baseFile, repository.Lookup<Blob>(conflict.Ancestor.Id).GetContentText());

                //var oursFile = Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, conflict.Ours.Path));
                var oursFile = Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, Path.ChangeExtension(conflict.Ours.Path, ".MINE" + Path.GetExtension(conflict.Ours.Path))));
                // We assume the local file already has the content we want, since we Stash before Merge
                File.WriteAllText(oursFile, repository.Lookup<Blob>(conflict.Ours.Id).GetContentText());

                var theirsFile = Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, Path.ChangeExtension(conflict.Theirs.Path, ".THEIRS" + Path.GetExtension(conflict.Theirs.Path))));
                File.WriteAllText(theirsFile, repository.Lookup<Blob>(conflict.Theirs.Id).GetContentText());

                var mergedFile = Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, conflict.Ours.Path));

                var indexOfDolar = mergeToolCmd.IndexOf("$");
                if (indexOfDolar == -1)
                    // There were no replacements, so we can't merge anything
                    return;

                var fileName = mergeToolCmd.Substring(0, indexOfDolar - 1).Trim();
                if (!fileName.StartsWith("\""))
                    fileName = "\"" + fileName;
                if (!fileName.EndsWith("\""))
                    fileName += "\"";

                var arguments = mergeToolCmd.Substring(indexOfDolar)
                    .Replace("$LOCAL", "\"" + oursFile + "\"")
                    .Replace("$REMOTE", "\"" + theirsFile + "\"")
                    .Replace("$BASE", "\"" + baseFile + "\"")
                    .Replace("$MERGED", "\"" + mergedFile + "\"");

#pragma warning restore CS8602 // Dereference of a possibly null reference.

                //Console.WriteLine(fileName + " " + arguments);

                try
                {
                    var process = Process.Start(fileName, arguments);
                    process.WaitForExit();
                    if (trustExitCode && process.ExitCode == 0)
                    {
                        // Automatically mark if exit code was success.
                        view.Source.SetMark(conflicts.IndexOf(conflict), true);
                        // This is not repainting the new checkmark :(
                        Application.MainLoop.Invoke(() =>
                        {
                            Redraw(Bounds);
                            Application.MainLoop.Driver.Wakeup();
                        });
                        
                        //Console.WriteLine("Merged");
                    }
                    else
                    {
                        //Console.WriteLine("Not merged");
                    }
                }
                finally
                {
                    if (!keepBackup)
                    {
                        File.Delete(baseFile);
                        File.Delete(oursFile);
                        File.Delete(theirsFile);
                    }
                }
            }
        }
    }
}
