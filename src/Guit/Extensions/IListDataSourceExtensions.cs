namespace Terminal.Gui
{
    static class IListDataSourceExtensions
    {
        /// <summary>
        /// Sets all items in the source as marked
        /// </summary>
        /// <param name="source"></param>
        public static void MarkAll(this IListDataSource source)
        {
            for (int i = 0; i < source.Count; i++)
                source.SetMark(i, true);
        }
    }
}