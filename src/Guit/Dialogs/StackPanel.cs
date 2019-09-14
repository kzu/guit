using System;
using System.Collections.Generic;
using System.Text;
using Terminal.Gui;

namespace Guit
{
    class StackPanel : View
    {
        public StackPanel(params View[] views)
        {
            for (int i = 1; i < views.Length; i++)
                views[i].Y = Pos.Bottom(views[i - 1]);

            Add(views);
        }
    }
}
