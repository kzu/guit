using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    class ShellWindow : Window, IRefreshPattern
    {
        readonly Tuple<int, int, int, int> margin;
        readonly IEnumerable<View> decorators;

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

        public void Refresh()
        {
            Content.Refresh();

            foreach (var decorator in decorators.OfType<IRefreshPattern>())
                decorator.Refresh();
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
    }
}