using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    class ShellWindow : Window, IRefreshPattern, IFilterPattern, ISelectPattern, ISupportInitializeNotification
    {
        readonly Tuple<int, int, int, int> margin;
        readonly IEnumerable<View> decorators;

        public event EventHandler? Initialized;

        public ShellWindow(
            string title,
            ContentView content,
            Tuple<int, int, int, int> margin,
            params View[] decorators)
            : base(title)
        {
            Content = content;

            this.margin = margin;
            this.decorators = decorators ?? Enumerable.Empty<View>();

            Add(content);
            Add(decorators);
        }

        public ContentView Content { get; }

        string[]? IFilterPattern.Filter
        {
            get => (Content as IFilterPattern)?.Filter;
            set
            {
                if (Content is IFilterPattern filterPattern)
                {
                    filterPattern.Filter = value;

                    Title = Content.Title;
                }
            }
        }

        public void Refresh()
        {
            Content.Refresh();

            foreach (var decorator in decorators.OfType<IRefreshPattern>())
                decorator.Refresh();
        }

        public void SelectAll(bool invertSelection = true)
        {
            (Content as ISelectPattern)?.SelectAll(invertSelection);

            foreach (var decorator in decorators.OfType<ISelectPattern>())
                decorator.SelectAll(invertSelection);
        }

        public override Rect Frame
        {
            get => base.Frame;
            set => base.Frame = new Rect(
                value.X - margin.Item1,
                value.Y - margin.Item2,
                value.Width - margin.Item3,
                value.Height - margin.Item4);
        }

        public bool IsInitialized { get; private set; }

        public override void Redraw(Rect bounds)
        {
            base.Redraw(bounds);

            foreach (var decorator in decorators)
                decorator.Redraw(bounds);
        }

        public override void LayoutSubviews()
        {
            foreach (var decorator in decorators)
                decorator.LayoutSubviews();

            base.LayoutSubviews();
        }

        public override string ToString() => Content.Title;

        void ISupportInitialize.BeginInit()
        {
            (Content as ISupportInitialize)?.BeginInit();

            foreach (var decorator in decorators.OfType<ISupportInitializeNotification>())
                decorator.BeginInit();
        }

        void ISupportInitialize.EndInit()
        {
            (Content as ISupportInitialize)?.EndInit();

            foreach (var decorator in decorators.OfType<ISupportInitializeNotification>())
                decorator.EndInit();

            IsInitialized = true;
            Initialized?.Invoke(this, new EventArgs());
        }
    }
}
