using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using Guit.Events;
using Merq;
using Terminal.Gui;

namespace Guit
{
    [Export]
    public class App : Toplevel
    {
        readonly IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> menuCommands;

        [ImportingConstructor]
        public App(
            CommitView mainWindow, 
            StatusBar status, 
            [ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> menuCommands,
            // Just importing the singletons causes them to be instantiated.
            [ImportMany] IEnumerable<ISingleton> singletons)
        {
            this.menuCommands = menuCommands;
            var commands = new View
            {
                Y = Pos.Bottom(mainWindow),
                Height = 1
            };

            View current = new Label("");
            commands.Add(current);
            foreach (var command in menuCommands.OrderBy(x => x.Metadata.Order))
            {
                current = new Button(command.Metadata.HotKey + " " + command.Metadata.DisplayName)
                {
                    CanFocus = false,
                    X = Pos.Right(current),
                    // TODO: improve execution, cancellation, etc.
                    Clicked = () => command.Value.ExecuteAsync(CancellationToken.None),
                };
                commands.Add(current);
            }

            status.Y = Pos.Bottom(commands);
            mainWindow.Height = Height - commands.Height - status.Height;

            Add(mainWindow, commands, status);
            singletons.ToList();
        }

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            var command = menuCommands.FirstOrDefault(x => keyEvent.Key == x.Metadata.HotKey);
            if (command != null)
                command.Value.ExecuteAsync(CancellationToken.None);

            return base.ProcessHotKey(keyEvent);
        }
    }
}