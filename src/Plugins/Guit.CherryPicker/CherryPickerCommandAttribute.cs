using System;
using Guit.Plugin.CherryPicker.Properties;

namespace Guit.Plugin.CherryPicker
{
    class CherryPickerCommandAttribute : MenuCommandAttribute
    {
        public CherryPickerCommandAttribute(string id, int key)
            : base(id, key, WellKnownViews.CherryPicker, typeof(Resources))
        { }
    }
}