using System.ComponentModel;

namespace Terminal.Gui
{
    /// <summary>
    /// Usability overloads for <see cref="IListDataSource"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IListDataSourceExtensions
    {
        /// <summary>
        /// Mark or unmark all items
        /// </summary>
        public static void MarkAll(this IListDataSource source, bool value)
        {
            for (var i = 0; i < source.Count; i++)
                source.SetMark(i, value);
        }

        /// <summary>
        /// Return true if all items are marked or unmarked
        /// </summary>
        public static bool All(this IListDataSource source, bool value)
        {
            for (int i = 0; i < source.Count; i++)
                if (source.IsMarked(i) != value)
                    return false;

            return true;
        }
    }
}