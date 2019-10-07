using System;
using System.Collections.Generic;

namespace Terminal.Gui
{
    public static class ViewExtensions
    {
        public static IEnumerable<View> TraverseSubViews(this View view) => view.TraverseSubViews(new List<View>());

        static IEnumerable<View> TraverseSubViews(this View view, List<View> views)
        {
            if (view.Subviews != null)
            {
                foreach (var subView in view.Subviews)
                {
                    views.Add(subView);

                    subView.TraverseSubViews(views);
                }
            }

            return views;
        }
    }
}