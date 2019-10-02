using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    class ShellWindow : Window
    {
        View globalCommands;
        View localCommands;

        public ShellWindow(string title, ContentView content, string context, CommandService commandService)
            : base(title)
        {
            Content = content;

            InitializeCommands(commandService, context);

            Add(content, globalCommands, localCommands);
        }

        public ContentView Content { get; }

        public override Rect Frame
        {
            get => base.Frame;
            // Make space for the bottom command bar
            set => base.Frame = new Rect(value.X, value.Y, value.Width, value.Height - 1);
        }

        public override void Redraw(Rect bounds)
        {
            base.Redraw(bounds);

            globalCommands.Redraw(bounds);
            localCommands.Redraw(bounds);
        }

        public override void LayoutSubviews()
        {
            ClearCommands();

            globalCommands.Y = localCommands.Y = Bounds.Height;
            localCommands.X = Frame.Width - localCommands.Subviews.Select(x => x.Frame.Width).Sum() - 2;

            base.LayoutSubviews();
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

        void ClearCommands()
        {
            for (var col = 0; col < Frame.Width; col++)
            {
                Application.Driver.Move(col, Frame.Height);
                Application.Driver.AddRune(' ');
            }
        }

        string GetKeyDisplayText(int key) => Enum.GetName(typeof(Key), (Key)key) ?? ((char)key).ToString();
    }
}