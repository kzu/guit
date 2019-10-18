using System;
using System.Collections.Generic;
using System.Linq;

namespace Guit
{
    public class ListViewItem<T>
    {
        Lazy<string> content;

        readonly int contentWidth;
        readonly IEnumerable<ColumnDefinition<T>> columnDefinitions;

        public ListViewItem(T item, int width, params ColumnDefinition<T>[] columnDefinitions)
        {
            Item = item;

            this.contentWidth = width;
            this.columnDefinitions = columnDefinitions ?? Enumerable.Empty<ColumnDefinition<T>>();

            content = new Lazy<string>(() =>
            {
                var values = this.columnDefinitions
                    .Select(columnDefinition =>
                    {
                        if (columnDefinition.Width != 0)
                            return Tuple.Create(columnDefinition.Width, columnDefinition.GetValue(Item));
                        else if (columnDefinition.WidthExpression?.EndsWith("%") == true && int.TryParse(columnDefinition.WidthExpression.TrimEnd('%'), out var coldWidthPercentage))
                            return Tuple.Create(contentWidth * coldWidthPercentage / 100, columnDefinition.GetValue(Item));
                        else if (columnDefinition.WidthExpression?.EndsWith("*") == true)
                            return Tuple.Create(int.MaxValue, columnDefinition.GetValue(Item));
                        else
                            return Tuple.Create(0, string.Empty);
                    }).ToList();

                var columnWidth = values.Select(x => x.Item1).Where(x => x != int.MaxValue).Sum();

                return string.Concat(values.Select(x =>
                    GetNormalizedString(x.Item2, x.Item1 == int.MaxValue ? contentWidth - columnWidth : x.Item1)));
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