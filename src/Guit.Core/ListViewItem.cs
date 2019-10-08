using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Guit
{
    public class ListViewItem<T>
    {
        Lazy<string> content;

        readonly int contentWidth;
        readonly IEnumerable<ListViewItemSelector<T>> selectors;

        public ListViewItem(T item, int width, params ListViewItemSelector<T>[] selectors)
        {
            Item = item;

            this.contentWidth = width;
            this.selectors = selectors ?? Enumerable.Empty<ListViewItemSelector<T>>();

            content = new Lazy<string>(() =>
            {
                var values = this.selectors
                    .Select(selector =>
                    {
                        if (selector.Width != 0)
                            return Tuple.Create(selector.Width, selector.GetValue(Item));
                        else if (selector.WidthExpression?.EndsWith("%") == true && int.TryParse(selector.WidthExpression.TrimEnd('%'), out var coldWidthPercentage))
                            return Tuple.Create(contentWidth * coldWidthPercentage / 100, selector.GetValue(Item));
                        else if (selector.WidthExpression?.EndsWith("*") == true)
                            return Tuple.Create(int.MaxValue, selector.GetValue(Item));
                        else
                            return Tuple.Create(0, string.Empty);
                    }).ToList();

                var totalSelectorWidth = values.Select(x => x.Item1).Where(x => x != int.MaxValue).Sum();

                return string.Concat(values.Select(x =>
                    GetNormalizedString(x.Item2, x.Item1 == int.MaxValue ? contentWidth - totalSelectorWidth : x.Item1)));
            });

        }

        public T Item { get; }

        public override string ToString() => content.Value;

        string GetNormalizedString(string value, int length)
        {
            if (value.Length < length)
                return string.Concat(value, new String(' ', length - value.Length));
            else if (value.Length >= length)
                return value.Substring(0, length - 4) + "... ";

            return value;
        }
    }
}