using Terminal.Gui;

namespace Guit
{
    public class StackPanel : View
    {
        public StackPanel(params View[] views)
        {
            for (int i = 1; i < views.Length; i++)
                views[i].Y = Pos.Bottom(views[i - 1]);

            Add(views);
        }

        public override void Add(View view)
        {
            if (Subviews.Count > 0)
                view.Y = Pos.Bottom(Subviews[Subviews.Count - 1]);

            base.Add(view);
        }
    }
}
