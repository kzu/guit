using LibGit2Sharp;
using Terminal.Gui;

namespace DotNetGit
{
    public class App : Window
    {
        public static Repository Repository { get; set; }

        public App() : base( ".NET Git")
        {
            X = 0;
            Y = 1;

            Width = Dim.Fill();
            Height = Dim.Fill();

            var menu = new MenuBar(new[] 
            {
                new MenuBarItem("_File", new [] {
                    new MenuItem("_Quit", "", () => Running = false)
                }),
                new MenuBarItem("_Sync", new [] {
                    new MenuItem("_Pull", "", null),
                    new MenuItem("P_ush", "", null),
                })
            });

            var staged = new FrameView("Staged")
            {
                X = 0,
                Y = Pos.Bottom(menu) + 1,
                Height = Dim.Percent(50),
            };
            staged.Add(new TextField("[commit message]"));

            var view = new CommitView
            {
                X = 0,
                Y = Pos.Bottom(staged),
                Height = Dim.Percent(100)
            };

            Add(menu, staged, view);
        }
    }
}