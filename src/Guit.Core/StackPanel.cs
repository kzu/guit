using System.Linq;
using Terminal.Gui;

namespace Guit
{
    public class StackPanel : View
    {
        readonly StackPanelOrientation orientation;

        public StackPanel(params View?[] views) : this(StackPanelOrientation.Vertical, views) { }

        public StackPanel(StackPanelOrientation orientation, params View?[] views)
        {
            this.orientation = orientation;

            foreach (var view in views)
            {
                if (view != null)
                    Add(view);
            }
        }

        public override void Add(View view)
        {
            if (Subviews.Count > 0)
            {
                switch (orientation)
                {
                    case StackPanelOrientation.Vertical:
                        view.Y = Pos.Bottom(Subviews[Subviews.Count - 1]);
                        break;
                    case StackPanelOrientation.Horizontal:
                        view.X = Pos.Right(Subviews[Subviews.Count - 1]);
                        break;
                }
            }

            base.Add(view);
        }
    }
}