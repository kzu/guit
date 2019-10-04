using System;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    class CommandBar : View
    {
        Window window;

        View globalCommands;
        View localCommands;

        public CommandBar(CommandService commandService, string context)
        {
            InitializeCommands(commandService, context);

            Add(globalCommands, localCommands);
        }

        void InitializeCommands(CommandService commandService, string context)
        {
            globalCommands = new StackPanel(
                StackPanelOrientation.Horizontal,
                commandService
                    .Commands
                    .Where(x => string.IsNullOrEmpty(x.Metadata.Context) && x.Metadata.Visible)
                    .OrderBy(x => x.Metadata.Order)
                    .Select(x => new Button(GetKeyDisplayText(x.Metadata.Key) + " " + x.Metadata.DisplayName) { CanFocus = false })
                    .ToArray());

            localCommands = new StackPanel(
                StackPanelOrientation.Horizontal,
                commandService
                    .Commands
                    .Where(x => x.Metadata.Context == context && x.Metadata.Visible)
                    .OrderBy(x => x.Metadata.Order)
                    .Select(x => new Button(GetKeyDisplayText(x.Metadata.Key) + " " + x.Metadata.DisplayName) { CanFocus = false })
                    .ToArray());
        }

        Window Window => window != null ? window : window = this.GetWindow();

        public override void Redraw(Rect region)
        {
            base.Redraw(region);

            globalCommands.Redraw(region);
            localCommands.Redraw(region);
        }

        public override void LayoutSubviews()
        {
            ClearCommands();

            globalCommands.Y = localCommands.Y = Window.Frame.Height;
            localCommands.X = Window.Frame.Width - localCommands.Subviews.Select(x => x.Frame.Width).Sum() - 2;

            base.LayoutSubviews();
        }

        void ClearCommands()
        {
            for (var col = 0; col < Window.Frame.Width; col++)
            {
                Application.Driver.Move(col, Window.Frame.Height);
                Application.Driver.AddRune(' ');
            }
        }

        string GetKeyDisplayText(int key) => Enum.GetName(typeof(Key), (Key)key) ?? ((char)key).ToString();
    }
}