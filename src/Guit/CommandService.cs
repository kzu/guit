using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit
{
    [Shared]
    [Export]
    class CommandService
    {
        readonly Dictionary<Tuple<int, string?>, Lazy<IMenuCommand, MenuCommandMetadata>> commands = new Dictionary<Tuple<int, string?>, Lazy<IMenuCommand, MenuCommandMetadata>>();
        readonly MainThread mainThread;

        [ImportingConstructor]
        public CommandService(
            IApp app,
            MainThread mainThread,
            [ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> commands,
            [ImportMany] IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views)
        {
            var allCommands = commands.Concat(
                views
                    .OrderBy(x => x.Metadata.Key)
                    .Select(x =>
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

            this.mainThread = mainThread;
        }

        public IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> Commands => commands.Values;

        public Task RunAsync(int hotKey, string? context)
        {
            if (!commands.TryGetValue(Tuple.Create(hotKey, context), out var command) &&
                !commands.TryGetValue(Tuple.Create(hotKey, default(string)), out command))
                return Task.CompletedTask;

            return ExecuteAsync(command);
        }

        Task ExecuteAsync(Lazy<IMenuCommand, MenuCommandMetadata> command, CancellationToken cancellation = default) =>
            Task.Run(async () =>
            {
                var error = default(Exception);
                using (var progress = new ReportStatusProgress(command.Metadata.DisplayName, EventStream.Default, mainThread))
                {
                    try
                    {
                        await command.Value.ExecuteAsync(cancellation);
                    }
                    catch (Exception ex)
                    {
                        error = ex;
                    }
                }

                if (error != null)
                    mainThread.Invoke(() => Application.Run(new MessageBox("Error", error.Message)));
            });
    }
}