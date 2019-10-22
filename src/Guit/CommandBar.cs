using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    class CommandBar : View
    {
        Window? window;

        View globalCommands;
        View localCommands;

        public CommandBar(CommandService commandService, string? context)
        {
            InitializeCommands(commandService, context);

            Add(globalCommands, localCommands);
        }

        void InitializeCommands(CommandService commandService, string? context)
        {
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
                    .ToArray());
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

            globalCommands.Redraw(region);
            localCommands.Redraw(region);
        }

        public override void LayoutSubviews()
        {
            ClearCommands();

            if (Window != null)
            {
                globalCommands.Y = localCommands.Y = Window.Frame.Height;
                localCommands.X = Window.Frame.Width - localCommands.Subviews.Select(x => x.Frame.Width).Sum() - 2;
            }

            base.LayoutSubviews();
        }

        void ClearCommands()
        {
            if (Window != null)
            {
                for (var col = 0; col < Window.Frame.Width; col++)
                {
                    Application.Driver.Move(col, Window.Frame.Height);
                    Application.Driver.AddRune(' ');
                }
            }
        }

        string GetKeyDisplayText(int key) => Enum.GetName(typeof(Key), (Key)key) ?? ((char)key).ToString();
    }
}