using System;
using System.Collections.Generic;
using System.Composition;
using Terminal.Gui;

namespace Guit.Plugin.Log
{
    [Shared]
    [Export]
    public class LogView : MainView
    {
        [ImportingConstructor]
        public LogView(
            [ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> globalCommands,
            [ImportMany(nameof(Log))] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> localCommands)
            : base("Log", globalCommands, localCommands)
        {
            Content = new View
            {
                new Label("// TODO: Log")
            };
        }
    }
}
