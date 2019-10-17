using System.Collections.Generic;

namespace Guit
{
    public interface IPluginManager
    {
        bool UseCorePlugins { get; set; }

        IEnumerable<PluginInfo> AvailablePlugins { get; }

        IEnumerable<PluginInfo> EnabledPlugins { get; set; }
    }
}