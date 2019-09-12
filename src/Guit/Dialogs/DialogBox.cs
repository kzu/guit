using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace Guit
{
    class DialogBox : Dialog
    {
        bool initialFocusSet;
        readonly bool useDefaultButtons;

        public DialogBox(string title, bool useDefaultButtons = false, bool initializeComponents = true)
            : base(title, 0, 0)
        {
            this.useDefaultButtons = useDefaultButtons;

            if (initializeComponents)
                InitializeComponents();
        }

        protected virtual void InitializeComponents()
        {
            Height = 15;
            Width = Dim.Fill(20);

            if (useDefaultButtons)
            {
                AddButton(AcceptButtonText, OnAcceptButtonClicked, true);
                AddButton(CancelButtonText, OnCancelButtonClicked);
            }
        }

        protected virtual void AddButton(string text, Action clickedAction, bool isDefault = false)
        {
            var button = new Button(text, isDefault);
            button.Clicked += clickedAction;

            AddButton(button);
        }

        protected virtual string AcceptButtonText => "OK";

        protected virtual string CancelButtonText => "Cancel";

        protected virtual void OnAcceptButtonClicked() => Close(result: true);

        protected virtual void OnCancelButtonClicked() => Close(result: false);

        public bool? Result { get; protected set; }

        protected View InitialFocusedView { get; set; }

        public bool? ShowDialog()
        {
            Application.Run(this);

            return Result;
        }

        public override bool ProcessKey(KeyEvent kb)
        {
            if (!useDefaultButtons && kb.KeyValue == (int)Key.Enter)
            {
                Close(true);
                return false;
            }

            return base.ProcessKey(kb);
        }

        protected virtual void Close(bool? result)
        {
            Result = result;
            Running = false;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            try
            {
                // Ensure to set initial focus to the specified view
                if (!initialFocusSet && InitialFocusedView != null)
                    SetFocus(InitialFocusedView);
            }
            finally
            {
                initialFocusSet = true;
            }
        }
    }
}