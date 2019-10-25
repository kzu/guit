using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Guit.Properties;
using Terminal.Gui;

namespace Guit
{
    [Shared]
#if DEBUG
    [Export]
    [ContentView(WellKnownViews.Plugins, '6', resources: typeof(Resources))]
#endif
    public class PluginsView : ContentView
    {
        readonly IPluginManager manager;

        List<PluginInfo> plugins = new List<PluginInfo>();
        ListView view;

        [ImportingConstructor]
        public PluginsView(IPluginManager manager)
            : base("Plugins")
        {
            this.manager = manager;

            view = new ListView(plugins)
            {
                AllowsMarking = true,
            };

            Content = view;
        }

        public override void Refresh()
        {
            base.Refresh();

            plugins = manager.AvailablePlugins.ToList();
            view.SetSource(plugins);

            var enabled = manager.EnabledPlugins.Select(x => x.Id).ToHashSet();

            for (var i = 0; i < view.Source.Count; i++)
            {
                if (enabled.Contains(plugins[i].Id))
                    view.Source.SetMark(i, true);
            }
        }

        public IEnumerable<PluginInfo> EnabledPlugins => plugins.Where((x, i) => view.Source.IsMarked(i));
    }
}
