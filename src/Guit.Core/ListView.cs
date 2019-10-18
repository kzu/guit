using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    public class ListView<T> : ListView, IFilterPattern
    {
        string[]? filter;

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

            var filteredValues = Values.ToList();
            if (filter?.Any() == true)
            {
                filteredValues = filteredValues.Where(x => selectors.Any(selector =>
                {
                    var value = selector.GetValue(x);

                    return filter.Any(filter => value?.Contains(filter) == true);
                })).ToList();
            }

            if (Frame.Width > 0)
                SetSource(filteredValues.Select(x => new ListViewItem<T>(x, Frame.Width - 4, selectors.ToArray())).ToList());

            foreach (var value in filteredValues)
                Source.SetMark(filteredValues.IndexOf(value), previousSelection.TryGetValue(value, out var marked) && marked);
        }

        string[]? IFilterPattern.Filter
        {
            get => filter;
            set
            {
                filter = value;

                SetFilter(filter);
            }
        }

        protected virtual void SetFilter(params string[]? filter) => RenderValues(Values);
    }
}