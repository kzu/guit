using System;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Log
{
    class ResetDialog : DialogBox
    {
        public ResetDialog() : base("Reset") { }

        public ResetMode ResetMode { get; set; } = ResetMode.Soft;

        protected override void EndInit()
        {
            Width = 80;
            Height = 15;

            Add(new RadioGroup(Enum.GetNames(typeof(ResetMode))),
                nameof(ResetMode),
                resetMode => (int)resetMode - 1,
                selectedIndex => (ResetMode)selectedIndex + 1);

            base.EndInit();
        }
    }
}