using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Terminal.Gui;
using System.Threading;
using Guit.Plugin;
using System.Composition;

namespace Guit
{
    [Shared]
    [Export]
    class CommandService
    {
        readonly IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> commands;

        [ImportingConstructor]
        public CommandService([ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> commands)
        {
            this.commands = commands;
        }

        IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> LocalCommands => commands.Where(x => !string.IsNullOrEmpty(x.Metadata.Context));

        IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> GlobalCommands => commands.Where(x => string.IsNullOrEmpty(x.Metadata.Context));

        public Task RunAsync(int hotKey, string context)
        {
            var command = LocalCommands.FirstOrDefault(x => hotKey == (int)x.Metadata.HotKey && x.Metadata.Context == context) ??
                    GlobalCommands.FirstOrDefault(x => hotKey == (int)x.Metadata.HotKey);

            if (command != null)
            {
                return Task.Run(async () =>
                {
                    using (var progress = new ReportStatusProgress(command.Metadata.DisplayName, EventStream.Default))
                        await command.Value.ExecuteAsync(CancellationToken.None);
                });
            }

            return Task.CompletedTask;
        }

        public View GetCommandsView(MainView view)
        {
            var commandsView = new View
            {
                Height = 1
            };

            var globals = new View();
            var spacer = new Label("||") { X = Pos.Right(globals), TextAlignment = TextAlignment.Centered };
            var locals = new View { X = Pos.Right(spacer) };

            spacer.Width = Dim.Width(view) - 2 - Dim.Width(globals) - Dim.Width(locals);
            commandsView.Add(globals, spacer, locals);

            View current = new Label("");
            globals.Add(current);
            globals.Width = Dim.Width(current);
            foreach (var command in GlobalCommands.OrderBy(x => x.Metadata.Order))
            {
                current = new Button(command.Metadata.HotKey + " " + command.Metadata.DisplayName)
                {
                    CanFocus = false,
                    X = Pos.Right(current),
                    // TODO: improve execution, cancellation, etc.
                    Clicked = () => command.Value.ExecuteAsync(CancellationToken.None),
                };
                globals.Add(current);
                // Not sure why we need to do this... seems like the containing view 
                // width should equal the width of its subviews automatically unless 
                // a different width is specified... 
                globals.Width += Dim.Width(current);
            }

            current = new Label("");
            locals.Add(current);
            locals.Width = Dim.Width(current);
            foreach (var command in LocalCommands.Where(x => x.Metadata.Context == view.Context).OrderBy(x => x.Metadata.Order))
            {
                current = new Button(command.Metadata.HotKey + " " + command.Metadata.DisplayName)
                {
                    CanFocus = false,
                    X = Pos.Right(current),
                    // TODO: improve execution, cancellation, etc.
                    Clicked = () => command.Value.ExecuteAsync(CancellationToken.None),
                };
                locals.Add(current);
                locals.Width += Dim.Width(current);
            }

            return commandsView;
        }
    }
}