using System.Collections.Generic;

namespace Terminal.Gui
{
    public static class ViewExtensions
    {
        public static IEnumerable<View> TraverseSubViews(this View view) =>
            view.Subviews.Traverse(TraverseKind.BreadthFirst, v => v.Subviews);

        public static T? GetSuperView<T>(this View view) where T : View =>
            view.SuperView is T superView ? superView : view.SuperView is null ? default : view.SuperView.GetSuperView<T>();
    }
}