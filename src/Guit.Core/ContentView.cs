using Terminal.Gui;

namespace Guit
{
    public abstract class ContentView : Window
    {
        View content;
        View commands;

        public ContentView(string title)
            : base(title)
        {
            Width = Dim.Fill();
            Height = Dim.Fill();

            // Seems like a bug in gui.cs since both are set with a margin of 2, 
            // which is 1 unnecessary extra value since X,Y are already 1.
            var content = Subviews[0];
            content.Width = Dim.Fill(1);
            content.Height = Dim.Fill(1);
        }

        public virtual string Context => null;

        public virtual void Refresh() { }

        protected View Content
        {
            get => content;
            set
            {
                content = value;
                Add(content);
            }
        }

        internal View Commands
        {
            get => commands;
            set
            {
                commands = value;

                commands.Y = Pos.Bottom(content);
                content.Height = Height - commands.Height;
                Add(commands);

                LayoutSubviews();
            }
        }
    }
}