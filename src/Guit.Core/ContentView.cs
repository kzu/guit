using System;
using System.Linq;
using System.ComponentModel;
using Terminal.Gui;

namespace Guit
{
    public abstract class ContentView : View, IRefreshPattern, ISupportInitializeNotification
    {
        string title;
        View? content;

        public event EventHandler<string>? TitleChanged;
        public event EventHandler? Initialized;

        public ContentView(string title)
            : base()
        {
            this.title = title;

            Width = Dim.Fill();
            Height = Dim.Fill();
        }

        public bool IsInitialized { get; private set; }

        public virtual void Refresh() { }

        public string Title
        {
            get => title;
            protected set
            {
                title = value;

                SetNeedsDisplay();
                TitleChanged?.Invoke(this, title);
            }
        }

        void ISupportInitialize.BeginInit() => BeginInit();

        void ISupportInitialize.EndInit()
        {
            EndInit();

            IsInitialized = true;
            Initialized?.Invoke(this, new EventArgs());
        }

        protected virtual void BeginInit() { }

        protected virtual void EndInit() { }

        protected View? Content
        {
            get => content;
            set
            {
                content = value;

                if (content != null)
                {
                    content.Width = Dim.Fill(1);
                    content.Height = Dim.Fill(1);

                    Add(content);
                }
            }
        }
    }
}