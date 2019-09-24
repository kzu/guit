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
        /// Sets all items in the source as marked
        /// </summary>
        public static void MarkAll(this IListDataSource source)
        {
            for (int i = 0; i < source.Count; i++)
                source.SetMark(i, true);
        }
    }
}