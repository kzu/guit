using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit
{
    class ResetDialog : DialogBox
    {
        readonly Dictionary<ResetMode, string> options =
            new Dictionary<ResetMode, string>
            {
                { ResetMode.Soft, "Soft" },
                {ResetMode.Hard, "Hard" },
                {ResetMode.Mixed, "Mixed" }
            };

        RadioGroup radioGroup;

        public ResetDialog()
            : base("Reset", useDefaultButtons: true)
        { }

        public ResetMode ResetMode => options.Keys.ToArray()[radioGroup.Selected];

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            Width = 80;
            Height = 15;

            radioGroup = new RadioGroup(options.Values.ToArray());

            InitialFocusedView = radioGroup;

            Add(radioGroup);
        }
    }
}