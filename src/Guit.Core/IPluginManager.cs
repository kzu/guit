using System.Collections.Generic;

namespace Guit
{
    public interface IPluginManager
    {
        bool UseCorePlugins { get; set; }

        IEnumerable<PluginInfo> Plugins { get; set; }
    }
}