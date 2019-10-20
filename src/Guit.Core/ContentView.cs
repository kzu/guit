using System;
using System.Linq;
using System.ComponentModel;
using Terminal.Gui;

namespace Guit
{
    public abstract class ContentView : View, IRefreshPattern, IFilterPattern, ISupportInitializeNotification
    {
        string baseTitle;
        string title;

        string[]? filter;
        View? content;

        public event EventHandler? Initialized;

        public ContentView(string title)
            : base()
        {
            baseTitle = this.title = title;

            Width = Dim.Fill();
            Height = Dim.Fill();
        }

        public bool IsInitialized { get; private set; }

        public string Title
        {
            get => title;
            protected set
            {
                baseTitle = title = value;

                RefreshTitle();
                SetNeedsDisplay();
            }
        }

        public virtual void Refresh() { }

        string[]? IFilterPattern.Filter
        {
            get => filter;
            set
            {
                filter = value;

                SetFilter(filter);

                RefreshTitle();
                SetNeedsDisplay();
            }
        }

        void RefreshTitle() =>
            title = filter?.Any() == true ? string.Format("{0} - filtering by {1}", baseTitle, string.Join(", ", filter)) : baseTitle;

        protected virtual void SetFilter(params string[]? filter)
        {
            foreach(var view in this.TraverseSubViews().OfType<IFilterPattern>())

            if (Content is IFilterPattern filterPattern)
                filterPattern.Filter = filter;
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