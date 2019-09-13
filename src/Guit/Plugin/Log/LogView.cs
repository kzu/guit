using System;
using System.Collections.Generic;
using System.Composition;
using Terminal.Gui;

namespace Guit.Plugin.Log
{
    [Shared]
    [Export]
    public class LogView : ContentView
    {
        [ImportingConstructor]
        public LogView()
            : base("Log")
        {
            Content = new View
            {
                new Label("// TODO: Log")
            };
        }

        public override string Context => nameof(Log);
    }
}
