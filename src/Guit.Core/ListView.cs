using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    public class ListView<T> : ListView
    {
        readonly IEnumerable<ListViewItemSelector<T>> selectors;

        public ListView(params ListViewItemSelector<T>[] selectors) : base(new List<T>())
        {
            this.selectors = selectors ?? Enumerable.Empty<ListViewItemSelector<T>>();
        }

        public void SetValues(IEnumerable<T> values) => RenderValues(values);

        public IEnumerable<T> Values { get; private set; } = Enumerable.Empty<T>();

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            RenderValues(Values);
        }

        void RenderValues(IEnumerable<T> values)
        {
            var previousValues = Values.ToList();
            var previousSelection = previousValues
                .ToDictionary(x => x, x => Source.IsMarked(previousValues.IndexOf(x)));

            Values = values;
            if (Frame.Width > 0)
                SetSource(Values.Select(x => new ListViewItem<T>(x, Frame.Width - 4, selectors.ToArray())).ToList());

            var currentValues = Values.ToList();
            foreach (var value in currentValues)
                Source.SetMark(currentValues.IndexOf(value), previousSelection.TryGetValue(value, out var marked) && marked);

        }
    }
}