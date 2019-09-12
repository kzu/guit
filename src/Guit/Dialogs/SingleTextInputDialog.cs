using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace Guit
{
    class SingleTextInputDialog : DialogBox
    {
        TextField valueText;
        string text;
        DialogBox parentDialog;

        public SingleTextInputDialog(string title, string text, DialogBox parentDialog = null)
            : base(title, useDefaultButtons: true, initializeComponents: false)
        {
            this.text = text;
            this.parentDialog = parentDialog;

            InitializeComponents();
        }

        protected override void InitializeComponents()
        {
            base.InitializeComponents();

            Width = 60;
            Height = 10;

            var label = new Label(text)
            {
                Y = 1,
                Width = Dim.Fill(2)
            };

            valueText = new TextField(string.Empty)
            {
                Y = Pos.Bottom(label)
            };

            InitialFocusedView = valueText;

            Add(label, valueText);
        }

        public string Text => valueText.Text.ToString();
    }
}