using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Plugin;
using Terminal.Gui;

namespace Guit
{
    [Shared]
    [Export]
    class CommandService
    {
        readonly Dictionary<Tuple<int, string>, Lazy<IMenuCommand, MenuCommandMetadata>> commands = new Dictionary<Tuple<int, string>, Lazy<IMenuCommand, MenuCommandMetadata>>();

        [ImportingConstructor]
        public CommandService(
            IApp app,
            [ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> commands,
            [ImportMany] IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views)
        {
            var allCommands = commands.Concat(
                views.Select(x =>
                    new Lazy<IMenuCommand, MenuCommandMetadata>(
                        () => new MenuCommandAdapter(x, app),
                        // We need a different metadata with a null context
                        // to convert the ContentView into a global command
                        new MenuCommandMetadata
                        {
                            DisplayName = x.Metadata.DisplayName,
                            Key = x.Metadata.Key,
                            Order = x.Metadata.Order,
                            Visible = true
                        })));

            foreach (var command in allCommands)
            {
                var key = Tuple.Create(command.Metadata.Key, command.Metadata.Context);

                if (!this.commands.TryGetValue(key, out var existingCommand) ||
                    existingCommand.Metadata.Order > command.Metadata.Order)
                    this.commands[key] = command;
            }
        }

        public Task RunAsync(int hotKey, string context)
        {
            if (!commands.TryGetValue(Tuple.Create(hotKey, context), out var command) &&
                !commands.TryGetValue(Tuple.Create(hotKey, default(string)), out command))
                return Task.CompletedTask;

            return ExecuteAsync(command);
        }

        public View GetCommands(ContentView view, string context)
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
            foreach (var command in commands.Values.Where(x => string.IsNullOrEmpty(x.Metadata.Context) && x.Metadata.Visible).OrderBy(x => x.Metadata.Order))
            {
                current = new Button(GetKeyDisplayText(command.Metadata.Key) + " " + command.Metadata.DisplayName)
                {
                    CanFocus = false,
                    X = Pos.Right(current),
                    Clicked = async () => await ExecuteAsync(command),
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
            foreach (var command in commands.Values.Where(x => x.Metadata.Context == context && x.Metadata.Visible).OrderBy(x => x.Metadata.Order))
            {
                current = new Button(GetKeyDisplayText(command.Metadata.Key) + " " + command.Metadata.DisplayName)
                {
                    CanFocus = false,
                    X = Pos.Right(current),
                    Clicked = async () => await ExecuteAsync(command),
                };
                locals.Add(current);
                locals.Width += Dim.Width(current);
            }


            return commandsView;
        }

        string GetKeyDisplayText(int key) => Enum.GetName(typeof(Key), (Key)key) ?? ((char)key).ToString();

        Task ExecuteAsync(Lazy<IMenuCommand, MenuCommandMetadata> command, CancellationToken cancellation = default) =>
            Task.Run(async () =>
            {
                using (var progress = new ReportStatusProgress(command.Metadata.DisplayName, EventStream.Default))
                    await command.Value.ExecuteAsync(cancellation);
            });
    }
}
