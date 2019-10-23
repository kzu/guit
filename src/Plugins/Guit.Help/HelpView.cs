using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Guit.Plugin.Help.Properties;
using Terminal.Gui;

namespace Guit.Plugin.Help
{
    [Shared]
    [Export]
    [ContentView(WellKnownViews.Help, '?', resources: typeof(Resources), Visible = false)]
    class HelpView : ContentView
    {
        readonly ListView<HelpEntry> listView;

        readonly IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views;
        readonly IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> commands;

        [ImportingConstructor]
        public HelpView(
            [ImportMany] IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views,
            [ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> commands)
            : base(Resources.Help)
        {
            this.views = views;
            this.commands = commands;

            listView = new ListView<HelpEntry>(
                    new ColumnDefinition<HelpEntry>(x => x.Type.ToString(), 12),
                    new ColumnDefinition<HelpEntry>(x => GetShortcutString(x.Shortcut), 6),
                    new ColumnDefinition<HelpEntry>(x => x.Name, 18),
                    new ColumnDefinition<HelpEntry>(x => x.AppliesTo, 15),
                    new ColumnDefinition<HelpEntry>(x => x.Description, "*"))
            {
                X = 1,
                AllowsMarking = false
            };

            Content = listView;
        }

        public override void Refresh()
        {
            base.Refresh();

            var viewEntries = views
                .Select(x => CreateHelpEntry(x.Metadata, HelpEntryType.View));

            var commandEntries = commands
                .Where(x => !x.Metadata.IsDynamic || (x.Value as IDynamicMenuCommand)?.IsEnabled == true)
                .Select(x => CreateHelpEntry(x.Metadata, HelpEntryType.Command));

            listView.SetValues(viewEntries
                .Concat(commandEntries)
                .OrderByDescending(x => x.Type == HelpEntryType.View)
                .ThenBy(x => x.AppliesTo)
                .ThenBy(x => x.Shortcut)
                .ThenBy(x => x.Name)
                .ToList());
        }

        string GetShortcutString(int shortcut) =>
            Enum.GetName(typeof(Key), (Key)shortcut) is string keyName ?
                (Key)shortcut switch
                {
                    Key.CursorLeft => "Left",
                    Key.CursorRight => "Right",
                    _ => keyName
                } : ((char)shortcut).ToString();

        HelpEntry CreateHelpEntry(MenuCommandMetadata metadata, HelpEntryType type) =>
            new HelpEntry(
                type,
                metadata.Key,
                metadata.DisplayName,
                metadata.Description,
                type == HelpEntryType.Command ? metadata.Context : default);
    }
}