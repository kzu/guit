using System;
using System.Collections.Generic;
using System.Composition;
using DotNetGit.Commands;
using Terminal.Gui;

namespace DotNetGit
{
    [Export]
    public class App : Toplevel
    {
        [ImportingConstructor]
        public App(Window mainWindow, [ImportMany] IEnumerable<Lazy<IMainCommand, IDictionary<string, object>>> mainCommands)
        {
            mainWindow.Height = Height - 1;
            var commands = new View();
            commands.Y = Pos.Bottom(mainWindow);
            commands.Height = 1;

            Label current = default;
            foreach (var command in mainCommands)
            {
                if (current != null)
                {
                    current = new Label(" | ")
                    {
                        X = Pos.Right(current)
                    };

                    commands.Add(current);
                }

                current = new Label(command.Metadata["HotKey"] + " " + command.Metadata["DisplayName"])
                {
                    X = current == null ? commands.X : Pos.Right(current)
                };

                commands.Add(current);
            }

            Add(mainWindow, commands);
        }
    }
}