using System;
using System.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Guit.Configuration;
using LibGit2Sharp;

namespace Guit.Plugin.Changes
{
    [Shared]
    [Export]
    class ToolConfigurationProvider
    {
        // TODO: verify this is also the case on Mac.
        /// <summary>
        /// %LocalAppData%\Programs\Microsoft VS Code\bin\code
        /// </summary>
        static readonly string DefaultCode = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Programs", "Microsoft VS Code", "bin", "code");

        readonly IRepository repository;
        readonly Lazy<string> editorTool;
        readonly Lazy<DiffTool> diffTool;
        readonly Lazy<MergeTool> mergeTool;

        [ImportingConstructor]
        public ToolConfigurationProvider(IRepository repository)
        {
            this.repository = repository;

            editorTool = new Lazy<string>(() => LocateEditor());

            diffTool = new Lazy<DiffTool>(() =>
            {
                DiffTool? tool = default;
                var toolName = repository.Config.GetValueOrDefault("diff.tool", default(string));
                if (!string.IsNullOrEmpty(toolName))
                {
                    // NOTE: we only support cmd-specified tools, since we need to do the replacements of 
                    // $BASE $LOCAL $REMOTE and $MERGED ourselves and otherwise wouldn't know the order.
                    tool = repository.Config.Read<DiffTool>("difftool", toolName);
                }

                if (string.IsNullOrEmpty(tool?.Cmd))
                {
                    var toolCmd = LocateCode() ?? 
                        throw new ArgumentException("Cannot determine default diff tool to use. Either set 'diff.tool' git config or install VS Code.");

                    if (toolCmd.IndexOf(' ') != -1)
                        toolCmd = "\"" + toolCmd + "\"";

                    // TODO: localize
                    toolCmd += " \"$LOCAL\" \"$REMOTE\" --diff";
                    tool = new DiffTool { Cmd = toolCmd };
                }

#pragma warning disable CS8603 // Possible null reference return.
                return tool;
#pragma warning restore CS8603 // Possible null reference return.
            });

            mergeTool = new Lazy<MergeTool>(() =>
            {
                MergeTool? tool = default;
                var toolName = repository.Config.GetValueOrDefault("merge.tool", default(string));
                if (!string.IsNullOrEmpty(toolName))
                {
                    // NOTE: we only support cmd-specified tools, since we need to do the replacements of 
                    // $BASE $LOCAL $REMOTE and $MERGED ourselves and otherwise wouldn't know the order.
                    tool = repository.Config.Read<MergeTool>("mergetool", toolName);
                }

                if (string.IsNullOrEmpty(tool?.Cmd))
                {
                    var toolCmd = LocateDiffMerge();
                    if (string.IsNullOrEmpty(toolCmd))
                        throw new ArgumentException("Cannot determine default merge tool to use. Either set 'merge.tool' git config or install Visual Studio 2017 or later.");

                    if (toolCmd.IndexOf(' ') != -1)
                        toolCmd = "\"" + toolCmd + "\"";

                    // TODO: localize
                    toolCmd += " \"$LOCAL\" \"$REMOTE\" \"$BASE\" \"$MERGED\" /t /m Mine Theirs";
                    tool = new MergeTool { Cmd = toolCmd };
                }

#pragma warning disable CS8603 // Possible null reference return.
                return tool;
#pragma warning restore CS8603 // Possible null reference return.
            });
        }

        private string LocateDiffMerge()
        {
            // Try using a running VS
            var devenv = Process.GetProcessesByName("devenv").FirstOrDefault();
            var vsDiffMerge = @"CommonExtensions\Microsoft\TeamFoundation\Team Explorer\vsDiffMerge.exe";
            if (devenv != null && File.Exists(Path.Combine(Path.GetDirectoryName(devenv.MainModule.FileName), vsDiffMerge)))
            {
                return Path.Combine(Path.GetDirectoryName(devenv.MainModule.FileName), vsDiffMerge);
            }
            else
            {
                // If no VS is running, just locate the latest vsDiffMerge we can find
                var vswhere = Path.Combine(Path.GetDirectoryName((Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).Location), "vswhere.exe");
                var process = Process.Start(new ProcessStartInfo(vswhere, $@"-requires Microsoft.VisualStudio.TeamExplorer -latest -find ""Common7\IDE\{vsDiffMerge}""")
                {
                    RedirectStandardOutput = true
                });

                return process.StandardOutput.ReadToEnd().Trim();
            }
        }

        private string LocateEditor()
        {
            // First try to use configured editor
            var editor = repository.Config.GetValueOrDefault<string>("core.editor");
            if (string.IsNullOrEmpty(editor) || editor.StartsWith("code "))
            {
                return LocateCode() ??
                    throw new ArgumentException("Could not find VS Code in %PATH%. Please use its full path in 'core.editor' git config.");
            }
            else
            {
                return editor;
            }
        }

        // TODO: do we need more fallbacks for Mac?
        private string? LocateCode() =>
            // Try 'code' from %PATH%
            Environment.GetEnvironmentVariable("PATH")
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Path.Combine(x, "code"))
                    .Where(x => File.Exists("code"))
                .FirstOrDefault() ??
            // Try default VSCode install path 
            (File.Exists(DefaultCode) ? DefaultCode : null);

        [Export("core.editor")]
        public string EditorTool => editorTool.Value;

        [Export]
        public DiffTool DiffTool => diffTool.Value;

        [Export]
        public MergeTool MergeTool => mergeTool.Value;
    }
}
