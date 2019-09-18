using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace Guit
{
    class SingleTextInputDialog : DialogBox
    {
        public SingleTextInputDialog(string title, string message)
            : base(title)
        {
            Message = message;
        }

        protected override void EndInit()
        {
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

            base.EndInit();
        }

        public string Message { get; set; }

        public string Text { get; set; }
    }
}