using System.Collections.Generic;
using System.Composition;
using Guit.Plugin.Changes;
using Terminal.Gui;

namespace Guit
{
    [Export]
    public class App : Toplevel
    {
        [ImportingConstructor]
        public App(
            ChangesView mainWindow, 
            //StatusBar status, 
            // Just importing the singletons causes them to be instantiated.
            [ImportMany] IEnumerable<ISingleton> singletons)
        {
            //status.Y = Pos.Bottom(mainWindow);
            //mainWindow.Height = Height - status.Height;
            Add(mainWindow);
        }
    }
}