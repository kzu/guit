using System.Composition;
using LibGit2Sharp;
using NStack;
using Terminal.Gui;

namespace DotNetGit
{
    [Shared]
    [Export(typeof(Window))]
    public class MainWindow : Window
    {
        [ImportingConstructor]
        public MainWindow(Repository repository) : base(null)
        {
            var staged = new FrameView("Staged")
            {
                X = 0,
                Y = 0,
                Height = Dim.Percent(50),
            };
            staged.Add(new TextField("[commit message]"));

            var view = new CommitView(repository)
            {
                X = 0,
                Y = Pos.Bottom(staged),
                Height = Dim.Percent(100)
            };

            Add(staged, view);
        }
    }
}
