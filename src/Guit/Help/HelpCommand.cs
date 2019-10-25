using System;
using System.Collections.Generic;
using System.Composition;
using Guit.Plugin.Help;
using Guit.Properties;

namespace Guit.Help
{
    [Shared]
    [Export]
    [MenuCommand(WellKnownViews.Help, '?', double.MaxValue, resources: typeof(Resources))]
    class HelpCommand : MenuCommand
    {
        [ImportingConstructor]
        public HelpCommand(MainThread mainThread, 
            [ImportMany] IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views,
            [ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> commands) 
            : base(() => mainThread.Invoke(() => new HelpDialog(views, commands).ShowDialog()))
        { }
    }
}
