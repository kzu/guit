using NStack;
using Terminal.Gui;

namespace DotNetGit
{
    public class App : Window
    {
        public App() : base(".NET Git")
        {
            X = 0;
            Y = 1;

            Width = Dim.Fill();
            Height = Dim.Fill();

            Add(new MenuBar(new[] 
            {
                new MenuBarItem("_File", new [] {
                    new MenuItem("_Open", "Opens a new git repository", () => { }),
                    new MenuItem("_Quit", "", () => Running = false)
                }),
                new MenuBarItem("_Sync", new [] {
                    new MenuItem("_Pull", "", null),
                    new MenuItem("P_ush", "", null),
                })
            }));
        }
    }
}
