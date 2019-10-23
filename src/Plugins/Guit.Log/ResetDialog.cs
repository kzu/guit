using System;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit.Plugin.Log
{
    class ResetDialog : DialogBox
    {
        readonly string message;

        public ResetDialog(string message) : base("Reset") {
            this.message = message;
        }

        public ResetMode ResetMode { get; set; } = ResetMode.Soft;

        protected override void EndInit()
        {
            Width = 80;
            Height = 15;

            Add(new StackPanel(
                    new Label(message),
                    new EmptyLine(),
                    Add(new RadioGroup(Enum.GetNames(typeof(ResetMode))),
                        nameof(ResetMode),
                        resetMode => (int)resetMode - 1,
                        selectedIndex => (ResetMode)selectedIndex + 1))
            {
                X = 1,
                Y = 1
            });

            base.EndInit();
        }
    }
}