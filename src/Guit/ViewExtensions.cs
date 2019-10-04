namespace Terminal.Gui
{
    static class ViewExtensions
    {
        public static Window GetWindow(this View view) =>
            view is Window window ? window : view != null ? GetWindow(view.SuperView) : null;
    }
}
