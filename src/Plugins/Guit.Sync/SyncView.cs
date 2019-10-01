using System.Composition;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    [Shared]
    [Export]
    [ContentView(nameof(Sync), '2')]
    public class SyncView : ContentView
    {
        [ImportingConstructor]
        public SyncView()
            : base("Sync")
        {
            Content = new View
            {
                new Label("// TODO: Sync")
            };
        }
    }
}
