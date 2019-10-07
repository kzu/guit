using System.Collections.Generic;

namespace Terminal.Gui
{
    public static class ViewExtensions
    {
        public static IEnumerable<View> TraverseSubViews(this View view) => 
            view.Subviews.Traverse(TraverseKind.BreadthFirst, v => v.Subviews);
    }
}