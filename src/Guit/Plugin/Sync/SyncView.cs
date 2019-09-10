using System;
using System.Collections.Generic;
using System.Composition;
using Terminal.Gui;

namespace Guit.Plugin.Sync
{
    [Shared]
    [Export]
    public class SyncView : MainView
    {
        [ImportingConstructor]
        public SyncView(
            [ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> globalCommands,
            [ImportMany(nameof(Sync))] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> localCommands)
            : base("Sync", globalCommands, localCommands)
        {
            Content = new View
            {
                new Label("// TODO: Sync")
            };
        }
    }
}
