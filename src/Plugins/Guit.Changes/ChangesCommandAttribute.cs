using System;
using Guit.Plugin.Changes.Properties;

namespace Guit.Plugin.Changes
{
    class ChangesCommandAttribute : MenuCommandAttribute
    {
        public ChangesCommandAttribute(string id, int key)
            : base(id, key, WellKnownViews.Changes, typeof(Resources))
        { }
    }
}