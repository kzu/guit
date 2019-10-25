using System;
using System.Collections.Generic;
using System.Text;

namespace Guit.Plugin.Help
{
    class HelpEntry
    {
        public HelpEntry(HelpEntryType type, int shortcut, string name, string description, string? appliesTo = default)
        {
            Type = type;
            Shortcut = shortcut;
            Name = name;
            Description = description;
            AppliesTo = appliesTo ?? string.Empty;
        }

        public HelpEntryType Type { get; }

        public int Shortcut { get; }

        public string Name { get; }

        public string Description { get; }

        public string AppliesTo { get; set; }
    }
}
