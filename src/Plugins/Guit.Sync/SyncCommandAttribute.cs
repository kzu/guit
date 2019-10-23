using System;
using Guit.Plugin.Sync.Properties;

namespace Guit.Plugin.Sync
{
    class SyncCommandAttribute : MenuCommandAttribute
    {
        public SyncCommandAttribute(string id, int key)
            : base(id, key, WellKnownViews.Sync, typeof(Resources))
        { }
    }
}