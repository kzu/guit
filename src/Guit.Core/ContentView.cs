using Terminal.Gui;

namespace Guit
{
    public abstract class ContentView : View, IRefreshPattern
    {
        View? content;

        public ContentView(string title)
            : base()
        {
            Title = title;

            Width = Dim.Fill();
            Height = Dim.Fill();
        }

        public string Title { get; }

        public virtual void Refresh() { }

        public virtual void SelectAll(bool invertSelection = true)
        {
            if (Content is ListView listView && listView?.AllowsMarking == true)
            {
                listView.Source.MarkAll(!(invertSelection && listView.Source.All(true)));
                listView.SetNeedsDisplay();
            }
        }

        protected View? Content
        {
            get => content;
            set
            {
                content = value;

                if (content != null)
                {
                    content.Width = Dim.Fill(1);
                    content.Height = Dim.Fill(1);

                    Add(content);
                }
            }
        }
    }
}