using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit.Plugin
{
    public abstract class MainView : Window
    {
        readonly IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> globalCommands;
        readonly IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> localCommands;

        readonly View commandsView;

        public MainView(
            string title,
            [ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> globalCommands,
            [ImportMany(nameof(Changes))] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> localCommands)
            : base(title)
        {
            this.globalCommands = globalCommands;
            this.localCommands = localCommands;

            Width = Dim.Fill();
            Height = Dim.Fill();

            // Seems like a bug in gui.cs since both are set with a margin of 2, 
            // which is 1 unnecessary extra value since X,Y are already 1.
            var contentView = Subviews[0];
            contentView.Width = Dim.Fill(1);
            contentView.Height = Dim.Fill(1);
            
            commandsView = new View
            {
                Height = 1
            };

            var globals = new View();
            var spacer = new Label("||") { X = Pos.Right(globals), TextAlignment = TextAlignment.Centered };
            var locals = new View { X = Pos.Right(spacer) };

            spacer.Width = Dim.Width(this) - 2 - Dim.Width(globals) - Dim.Width(locals);
            commandsView.Add(globals, spacer, locals);

            View current = new Label("");
            globals.Add(current);
            globals.Width = Dim.Width(current);
            foreach (var command in globalCommands.OrderBy(x => x.Metadata.Order))
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
            foreach (var command in localCommands.OrderBy(x => x.Metadata.Order))
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
        }

        protected View Content
        {
            set
            {
                commandsView.Y = Pos.Bottom(value);
                value.Height = Height - 1;
                Add(value, commandsView);
                LayoutSubviews();
            }
        }

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            var command = localCommands.FirstOrDefault(x => keyEvent.Key == x.Metadata.HotKey) ??
                globalCommands.FirstOrDefault(x => keyEvent.Key == x.Metadata.HotKey);

            if (command != null)
            {
                Task.Run(async () =>
                {
                    using (var progress = new ReportStatusProgress(command.Metadata.DisplayName, EventStream.Default))
                        await command.Value.ExecuteAsync(CancellationToken.None);
                });
            }

            return base.ProcessHotKey(keyEvent);
        }
    }
}
