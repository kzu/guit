using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    public class ListView<T> : ListView, IFilterPattern, IViewPattern, ISelectPattern
    {
        string[]? filter;

        readonly IEnumerable<ColumnDefinition<T>> columnDefinitions;

        public ListView(params ColumnDefinition<T>[] columnDefinitions) : base(new List<T>())
        {
            this.columnDefinitions = columnDefinitions;

            if (!this.columnDefinitions.Any())
                this.columnDefinitions = new[] { new ColumnDefinition<T>(x => x?.ToString() ?? string.Empty, "*") };
        }

        public IEnumerable<T> MarkedEntries => Values.Where((x, i) => Source.IsMarked(i));

        public T SelectedEntry =>
            SelectedItem >= 0 && SelectedItem < Values.Count() ?
                Values.ElementAt(SelectedItem) : default;

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
                filteredValues = filteredValues.Where(x => columnDefinitions.Any(columnDefinition =>
                {
                    var value = columnDefinition.GetValue(x)?.ToLowerInvariant();

                    return filter.Any(filter => value?.Contains(filter.ToLowerInvariant()) == true);
                })).ToList();
            }

            if (Frame.Width > 0)
                SetSource(filteredValues.Select(x => new ListViewItem<T>(x, Frame.Width - 4, columnDefinitions.ToArray())).ToList());

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

        void ISelectPattern.SelectAll(bool invertSelection) => SelectAll(invertSelection);

        protected virtual void SelectAll(bool invertSelection)
        {
            if (AllowsMarking)
            {
                Source.MarkAll(!(invertSelection && Source.All(true)));
                SetNeedsDisplay();
            }
        }

        public void View() => (SelectedEntry as IViewPattern)?.View();
    }
}