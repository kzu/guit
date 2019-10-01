using System;
using Terminal.Gui;

namespace Guit
{
    public class MenuCommandMetadata
    {
        public string DisplayName { get; set; }
        public int Key { get; set; }
        public double Order { get; set; }
        public string Context { get; set; }
        public bool Visible { get; set; }
    }
}
