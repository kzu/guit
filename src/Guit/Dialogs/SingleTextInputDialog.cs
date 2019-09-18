using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace Guit
{
    class SingleTextInputDialog : DialogBox
    {
        public SingleTextInputDialog(string title, string message)
            : base(title, useDefaultButtons: true)
        {
            Message = message;
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            Width = 60;
            Height = 10;

            var messageLabel = Bind(
                new Label(string.Empty)
                {
                    Y = 1,
                    Width = Dim.Fill(2)
                }, nameof(Message));

            var textField = Bind(new TextField(string.Empty), nameof(Text));

            InitialFocusedView = textField;

            Add(new StackPanel(messageLabel, textField));
        }

        public string Message { get; set; }

        public string Text { get; set; }
    }
}