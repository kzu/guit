using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit.Plugin
{
    public abstract class MainView : Window
    {
        View content;
        View commandsView;

        public MainView(string title)
            : base(title)
        {
            Width = Dim.Fill();
            Height = Dim.Fill();
            
            // Seems like a bug in gui.cs since both are set with a margin of 2, 
            // which is 1 unnecessary extra value since X,Y are already 1.
            var contentView = Subviews[0];
            contentView.Width = Dim.Fill(1);
            contentView.Height = Dim.Fill(1);
        }

        public virtual string Context => null;

        protected View Content 
        {
            get => content;
            set
            {
                content = value;

                Add(content);
            }
        }

        public View CommandsView
        {
            get => commandsView;
            set
            {
                commandsView = value;

                commandsView.Y = Pos.Bottom(content);
                content.Height = Height - commandsView.Height;
                Add(commandsView);

                LayoutSubviews();
            }
        }
    }
}