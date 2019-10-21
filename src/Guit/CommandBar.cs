using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    class CommandBar : View //, IRefreshPattern
    {
        Window? window;

        readonly CommandService commandService;
        readonly string? context;

        View globalCommands;
        View localCommands;

        public CommandBar(CommandService commandService, string? context)
        {
            this.commandService = commandService;
            this.context = context;

            Refresh();
        }

        IEnumerable<View> RenderCommand(Lazy<IMenuCommand, MenuCommandMetadata> command)
        {
            yield return Separator;
            yield return new Label(GetKeyDisplayText(command.Metadata.Key))
            {
                TextColor = Application.Driver.MakeAttribute(Color.BrightYellow, Color.Black),
            };
            yield return Space;
            yield return new Label(command.Metadata.DisplayName);
        }

        View Space => new Label(" ");

        View Separator => new Label(" | ");

        bool IsVisible(Lazy<IMenuCommand, MenuCommandMetadata> command)
        {
            if (command.Metadata.IsDynamic && command.Value is IDynamicMenuCommand dynamicCommand)
                return dynamicCommand.IsVisible;

            return command.Metadata.DefaultVisible;
        }

        Window? Window => window != null ? window : window = this.GetWindow();

        public override void Redraw(Rect region)
        {
            base.Redraw(region);

            // TODO: if we don't do this, nothing gets rendered.
            globalCommands.Redraw(region);
            localCommands.Redraw(region);
        }

        public override void LayoutSubviews()
        {
            if (Window != null)
            {
                // Clear previous rendering of the command bar
                for (var col = 0; col < Window.Frame.Width; col++)
                {
                    Application.Driver.Move(col, Window.Frame.Height);
                    Application.Driver.AddRune(' ');
                }

                globalCommands.Y = localCommands.Y = Window.Frame.Height;
                localCommands.X = Window.Frame.Width - localCommands.Subviews.Select(x => x.Frame.Width).Sum() - 2;
            }

            base.LayoutSubviews();
        }

        public void Refresh()
        {
            InitializeCommands();
            // None of the following causes the actual menu to be refreshed properly
            LayoutSubviews();
            SetNeedsDisplay();
            //Redraw(Frame);
            //Application.Driver.Refresh();
        }

        string GetKeyDisplayText(int key) => Enum.GetName(typeof(Key), (Key)key) ?? ((char)key).ToString();

        void InitializeCommands()
        {
            RemoveAll();

            globalCommands = new StackPanel(
                StackPanelOrientation.Horizontal,
                commandService
                    .Commands
                    .Where(x => string.IsNullOrEmpty(x.Metadata.Context) && IsVisible(x))
                    .OrderBy(x => x.Metadata.Order)
                    .SelectMany(RenderCommand)
                    // Skip first separator
                    .Skip(1)
                    .ToArray())
            {
                ColorScheme = Colors.Base,
            };

            localCommands = new StackPanel(
                StackPanelOrientation.Horizontal,
                commandService
                    .Commands
                    .Where(x => x.Metadata.Context == context && IsVisible(x))
                    .OrderBy(x => x.Metadata.Order)
                    .SelectMany(RenderCommand)
                    // Skip first separator
                    .Skip(1)
                    .ToArray())
            {
                ColorScheme = Colors.Base,
            };

            Add(globalCommands, localCommands);
        }
    }
}