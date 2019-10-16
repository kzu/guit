using System;
using System.ComponentModel;
using Terminal.Gui;

namespace Guit
{
    public abstract class ContentView : View, IRefreshPattern, ISupportInitializeNotification
    {
        View? content;

        public event EventHandler? Initialized;

        public ContentView(string title)
            : base()
        {
            Title = title;

            Width = Dim.Fill();
            Height = Dim.Fill();
        }

        public bool IsInitialized { get; private set; }

        public string Title { get; }

        public virtual void Refresh() { }

        public virtual void SelectAll(bool invertSelection = true)
        {
            if (Content is ListView listView && listView?.AllowsMarking == true)
            {
                listView.Source.MarkAll(!(invertSelection && listView.Source.All(true)));
                listView.SetNeedsDisplay();
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