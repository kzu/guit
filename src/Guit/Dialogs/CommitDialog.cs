using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using NStack;
using Terminal.Gui;

namespace Guit
{
    class CommitDialog : DialogBox
    {
        TextView messageText;

        public CommitDialog() : base("Commit", useDefaultButtons: true)
        { }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            Height = Dim.Fill(5);

            messageText = new TextView()
            {
                Text = string.Empty,
                X = 1,
                Y = 1,
                Width = Dim.Fill(1),
                Height = Dim.Height(this) - 7
            };

            InitialFocusedView = messageText;

            AddButton("Commit To", OnNewBranchClicked);
            Add(messageText);
        }

        public string Message => messageText.Text.ToString();

        public string NewBranchName { get; private set; }

        void OnNewBranchClicked()
        {
            var dialog = new SingleTextInputDialog("New Branch Name", "Commit to:", this);

            if (dialog.ShowDialog() == true)
            {
                NewBranchName = dialog.Text;
                Close(true);
            }
        }
    }
}