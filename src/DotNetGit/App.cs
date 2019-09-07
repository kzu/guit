using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using DotNetGit.Commands;
using Merq;
using Terminal.Gui;

namespace DotNetGit
{
    [Export]
    public class App : Toplevel
    {
        IEnumerable<Lazy<IMainCommand, IDictionary<string, object>>> mainCommands;

        [ImportingConstructor]
        public App(Window mainWindow, StatusBar outputPane, 
            IEventStream eventStream,
            [ImportMany] IEnumerable<Lazy<IMainCommand, IDictionary<string, object>>> mainCommands)
        {
            this.mainCommands = mainCommands;
            mainWindow.Height = Height - 4;
            outputPane.Y = Pos.Bottom(mainWindow);

            var commands = new View
            {
                Y = Pos.Bottom(outputPane),
                Height = 1
            };

            View current = new Label("");
            commands.Add(current);
            foreach (var command in mainCommands)
            {
                //current = AddSeparator(commands, current);
                current = new Button(command.Metadata["HotKey"] + " " + command.Metadata["DisplayName"])
                {
                    CanFocus = false,
                    X = Pos.Right(current),
                    // TODO: improve execution, cancellation, etc.
                    Clicked = () => command.Value.ExecuteAsync(CancellationToken.None),
                };
                commands.Add(current);
            }

            AddSeparator(commands, current);
            Add(mainWindow, outputPane, commands);
        }

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            var command = mainCommands.FirstOrDefault(x => keyEvent.Key == (Key)x.Metadata["HotKey"]);
            if (command != null)
                command.Value.ExecuteAsync(CancellationToken.None);

            return base.ProcessHotKey(keyEvent);
        }

        static View AddSeparator(View commands, View current)
        {
            current = new Label("|")
            {
                X = Pos.Right(current)
            };
            commands.Add(current);
            return current;
        }
    }
}