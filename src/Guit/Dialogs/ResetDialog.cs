using System;
using System.Linq;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit
{
    class ResetDialog : DialogBox
    {
        public ResetDialog()
            : base("Reset", useDefaultButtons: true)
        { }

        public ResetMode ResetMode { get; set; } = ResetMode.Soft;

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            Width = 80;
            Height = 15;

            InitialFocusedView = Add(
                new RadioGroup(Enum.GetNames(typeof(ResetMode))),
                nameof(ResetMode),
                resetMode => (int)resetMode - 1,
                selectedIndex => (ResetMode)selectedIndex + 1);
        }
    }
}