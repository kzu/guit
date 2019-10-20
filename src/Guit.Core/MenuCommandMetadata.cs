using System;
using Terminal.Gui;

namespace Guit
{
    public class MenuCommandMetadata
    {
        public string Id { get; set; }
        public string DisplayName { get; set; }
        public int Key { get; set; }
        public double Order { get; set; }
        public string? Context { get; set; }
        public bool DefaultVisible { get; set; }
        public bool ReportProgress { get; set; } = true;
        public bool IsDynamic { get; set; }
    }
}
