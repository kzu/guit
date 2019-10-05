﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using Terminal.Gui;

namespace Guit
{
    /// <summary>
    /// Simple <see cref="Dialog"/> window containing default <c>Accept</c> and 
    /// <c>Cancel</c> buttons that can be customized.
    /// </summary>
    public class DialogBox : Dialog, ISupportInitializeNotification
    {
        bool initialFocusSet;

        public DialogBox(string title) : base(title, 0, 0) { }

        /// <summary>
        /// Event raised when the dialog is initialized, which only happens once 
        /// before displaying it.
        /// </summary>
        public event EventHandler Initialized = (sender, e) => { };

        /// <summary>
        /// Whether the dialog has been initialized.
        /// </summary>
        public bool IsInitialized { get; private set; }

        void ISupportInitialize.BeginInit() => BeginInit();

        void ISupportInitialize.EndInit() => EndInit();

        /// <summary>
        /// Begins the initialization of the dialog prior to display for the first time.
        /// </summary>
        protected virtual void BeginInit()
        {
            Height = 15;
            Width = Dim.Fill(20);

            if (ShowDefaultButtons)
            {
                AddButton(AcceptButtonText, OnAcceptButtonClicked, true);
                AddButton(CancelButtonText, OnCancelButtonClicked);
            }
        }

        /// <summary>
        /// Finishes the initialization of the dialog prior to display for the first time.
        /// Sets the <see cref="IsInitialized"/> and raises <see cref="Initialized"/>.
        /// </summary>
        protected virtual void EndInit()
        {
            Initialized?.Invoke(this, EventArgs.Empty);
            IsInitialized = true;
        }

        protected virtual void AddButton(string text, Action clickedAction, bool isDefault = false)
        {
            var button = new Button(text, isDefault);
            button.Clicked += clickedAction;

            AddButton(button);
        }

        /// <summary>
        /// Whether to add the default buttons.
        /// </summary>
        protected bool ShowDefaultButtons { get; set; } = true;

        protected virtual string AcceptButtonText => "OK";

        protected virtual string CancelButtonText => "Cancel";

        protected virtual void OnAcceptButtonClicked() => Close(result: true);

        protected virtual void OnCancelButtonClicked() => Close(result: false);

        public bool? Result { get; protected set; }

        protected View? InitialFocusedView { get; set; }

        public bool? ShowDialog()
        {
            Application.Run(this);

            return Result;
        }

        public override bool ProcessKey(KeyEvent kb)
        {
            if (!ShowDefaultButtons && kb.KeyValue == (int)Key.Enter)
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

        readonly List<Binding> bindings = new List<Binding>();

        public T Bind<T>(
            T control,
            string propertyName,
            Func<object, object>? convertTo = null,
            Func<object, object>? convertFrom = null) where T : View
        {
            var binding = new Binding(control, this, propertyName, convertTo, convertFrom);

            binding.Register();
            binding.Run();

            bindings.Add(binding);

            return control;
        }

        public T Add<T>(
            T control,
            string propertyNameBinding,
            Func<object, object>? convertTo = null,
            Func<object, object>? convertFrom = null) where T : View
        {
            Add(control);

            return Bind(control, propertyNameBinding, convertTo, convertFrom);
        }

        public override void WillPresent()
        {
            base.WillPresent();

            foreach (var binding in bindings)
                binding.Run();
        }
    }
}