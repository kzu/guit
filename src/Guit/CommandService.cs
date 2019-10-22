using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using Merq;
using Terminal.Gui;

namespace Guit
{
    [Shared]
    [Export]
    [Export(typeof(ICommandService))]
    class CommandService : ICommandService
    {
        readonly Dictionary<Tuple<int, string?>, Lazy<IMenuCommand, MenuCommandMetadata>> commands = new Dictionary<Tuple<int, string?>, Lazy<IMenuCommand, MenuCommandMetadata>>();
        readonly MainThread mainThread;
        readonly IEventStream eventStream;
        readonly Selection selection;

        [ImportingConstructor]
        public CommandService(
            IShell app,
            MainThread mainThread,
            IEventStream eventStream,
            Selection selection,
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
                            DefaultVisible = true,
                            ReportProgress = false
                        })));

            foreach (var command in allCommands)
            {
                var key = Tuple.Create(command.Metadata.Key, command.Metadata.Context);

                if (!this.commands.TryGetValue(key, out var existingCommand) ||
                    existingCommand.Metadata.Order > command.Metadata.Order)
                    this.commands[key] = command;
            }

            this.mainThread = mainThread;
            this.eventStream = eventStream;
            this.selection = selection;
        }

        public IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> Commands => commands.Values;

        public Task RunAsync(int hotKey, string? context, object? parameter = null, CancellationToken cancellation = default)
        {
            if (!commands.TryGetValue(Tuple.Create(hotKey, context), out var command) &&
                !commands.TryGetValue(Tuple.Create(hotKey, default(string)), out command))
                return Task.CompletedTask;

            return ExecuteAsync(command, parameter ?? selection.Current, cancellation);
        }

        public Task RunAsync(string commandId, object? parameter = null, CancellationToken cancellation = default)
        {
            var command = commands.Values.FirstOrDefault(x => x.Metadata.Id == commandId);

            if (command != null)
                return ExecuteAsync(command, parameter ?? selection.Current, cancellation);

            return Task.CompletedTask;
        }

        Task ExecuteAsync(Lazy<IMenuCommand, MenuCommandMetadata> command, object? parameter = null, CancellationToken cancellation = default) =>
            Task.Run(async () =>
            {
                var commandEnabled = true;

                if (command.Metadata.IsDynamic && command.Value is IDynamicMenuCommand dynamicCommand)
                    commandEnabled = dynamicCommand.IsEnabled;

                if (commandEnabled)
                {
                    using (var progress = command.Metadata.ReportProgress ?
                        (IDisposable)new ReportStatusProgress(command.Metadata.DisplayName, EventStream.Default, mainThread) : new NullProgressStatus())
                    {
                        try
                        {
                            await command.Value.ExecuteAsync(parameter, cancellation);
                        }
                        catch (Exception ex)
                        {
                            mainThread.Invoke(() =>
                            {
                                eventStream.Push(Status.Failed());

                                Application.Run(new MessageBox("Error", ex.Message));
                            });
                        }
                    }

                    if (command.Value is IAfterExecuteCallback afterCallback)
                        await afterCallback.AfterExecuteAsync(cancellation);
                }
            });
    }
}