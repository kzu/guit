using System;
using System.Linq;
using System.ComponentModel;
using Terminal.Gui;

namespace Guit
{
    public abstract class ContentView : View, IRefreshPattern, IFilterPattern, ISelectPattern, ISupportInitializeNotification
    {
        string[]? filter;
        string baseTitle;
        View? content;

        public event EventHandler? Initialized;

        public ContentView(string title)
            : base()
        {
            baseTitle = title;
            Title = title;

            Width = Dim.Fill();
            Height = Dim.Fill();
        }

        public bool IsInitialized { get; private set; }

        public string Title { get; private set; }

        public virtual void Refresh() { }

        string[]? IFilterPattern.Filter
        {
            get => filter;
            set
            {
                filter = value;
                Title = filter?.Any() == true ? string.Format("{0} (filtering by {1})", baseTitle, string.Join(", ", filter)) : baseTitle;

                SetFilter(filter);
            }
        }

        protected virtual void SetFilter(params string[]? filter)
        {
            if (Content is IFilterPattern filterPattern)
                filterPattern.Filter = filter;
        }

        void ISelectPattern.SelectAll(bool invertSelection) => SelectAll(invertSelection);

        protected virtual void SelectAll(bool invertSelection = true)
        {
            if (Content is ISelectPattern listView)
                listView.SelectAll();
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