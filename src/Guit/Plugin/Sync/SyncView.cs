using System.Composition;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    [Shared]
    [Export]
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

        public override string Context => nameof(Sync);
    }
}
