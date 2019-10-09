using System.Composition;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    [Shared]
#if DEBUG
    [Export]
    [ContentView("Plugins", '6')]
#endif
    public class PluginsView : ContentView
    {
        readonly IPluginManager manager;

        ListView view;

        [ImportingConstructor]
        public PluginsView(IPluginManager manager)
            : base("Plugins")
        {
            this.manager = manager;

            view = new ListView(manager.Plugins.ToList());

            Content = view;
        }
    }
}
