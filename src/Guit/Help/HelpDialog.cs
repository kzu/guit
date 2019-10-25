using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using Guit.Properties;
using Terminal.Gui;

namespace Guit.Plugin.Help
{
    class HelpDialog : DialogBox
    {
        readonly IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views;
        readonly IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> commands;
        readonly Dictionary<int, (string Description, string? Context)> viewMap;
        readonly Dictionary<(int Key, string? Context), string> cmdMap;
        readonly int contentWidth;
        readonly int maxHeight;
        readonly Label description;

        [ImportingConstructor]
        public HelpDialog(
            [ImportMany] IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views,
            [ImportMany] IEnumerable<Lazy<IMenuCommand, MenuCommandMetadata>> commands)
            : base(Resources.Help)
        {
            this.views = views;
            this.commands = commands;

            viewMap = views.ToDictionary(x => x.Metadata.Key, x => (x.Metadata.Description, x.Metadata.Context));
            cmdMap = commands.ToDictionary(x => (x.Metadata.Key, x.Metadata.Context), x => x.Metadata.Description);

            Buttons = DialogBoxButton.Ok;

            maxHeight = -1;

            var nameWidth = commands.Max(x => x.Metadata.DisplayName.Length) + 3;
            var keyWidth = commands.Max(x => GetShortcutString(x.Metadata.Key).Length);

            contentWidth = nameWidth + keyWidth + 2;

            Width = (contentWidth * 2) + 11;

            var height = 8;

            var left = new View
            {
                Width = contentWidth,
                Y = 1,
                X = 2,
            };

            var right = new View
            {
                Width = contentWidth,
                X = contentWidth + left.X + 3,
                Y = left.Y,
            };

            var commandsByView = commands.GroupBy(cmd => views.FirstOrDefault(view => view.Metadata.Id == cmd.Metadata.Context))
                .OrderBy(x => x?.Key?.Metadata?.Order ?? -1)
                .ThenBy(x => x?.Key?.Metadata?.Key ?? 0);

            var pageSize = commandsByView.Count();
            if (Math.Abs(pageSize / 2) == pageSize / 2)
                pageSize = pageSize / 2;
            else
                pageSize = Math.Abs(pageSize / 2) + 1;

            var panel = left;
            var index = -1;
            foreach (var group in commandsByView)
            {
                index++;
                if (index >= pageSize)
                {
                    panel = right;
                    if (maxHeight == -1)
                        maxHeight = height;
                }

                var view = group.Key;
                if (view == null)
                {
                    view = new Lazy<ContentView, MenuCommandMetadata>(new MenuCommandMetadata
                    {
                        Key = -1,
                        DisplayName = Resources.HelpDialog_GlobalCommands,
                        Description = Resources.HelpDialog_GlobalCommandsDescription
                    });
                }

                panel.Add(new Label(view.Metadata.DisplayName)
                {
                    TextColor = Terminal.Gui.Attribute.Make(Color.Blue, Color.Gray),
                    Width = view.Metadata.Key >= 0 ? nameWidth : contentWidth,
                    Y = panel.Subviews.Count == 0 ? 0 : Pos.Bottom(panel.Subviews[panel.Subviews.Count - 1]),
                });
                panel.Add(new Label(" " + GetShortcutString(view.Metadata.Key) + " ")
                {
                    TextColor = Terminal.Gui.Attribute.Make(Color.White, Color.Black),
                    Y = panel.Subviews[panel.Subviews.Count - 1].Y,
                    X = Pos.Right(panel.Subviews[panel.Subviews.Count - 1]),
                });
                panel.Add(new Label(new string('-', contentWidth))
                {
                    Width = contentWidth,
                    Y = Pos.Bottom(panel.Subviews[panel.Subviews.Count - 1]),
                });

                height += 2;

                foreach (var command in group.OrderBy(c => c.Metadata.Order).ThenBy(c => c.Metadata.Key))
                {
                    panel.Add(new Label(command.Metadata.DisplayName)
                    {
                        Width = nameWidth,
                        Y = Pos.Bottom(panel.Subviews[panel.Subviews.Count - 1]),
                    });
                    panel.Add(new Label(" " + GetShortcutString(command.Metadata.Key) + " ")
                    {
                        TextColor = Application.Driver.MakeAttribute(Color.White, Color.Black),
                        Y = panel.Subviews[panel.Subviews.Count - 1].Y,
                        X = Pos.Right(panel.Subviews[panel.Subviews.Count - 1])
                    });

                    height++;
                }

                panel.Add(new Label(new string(' ', contentWidth))
                {
                    Width = contentWidth,
                    Y = Pos.Bottom(panel.Subviews[panel.Subviews.Count - 1]),
                });
                panel.Add(new Label(new string(' ', contentWidth))
                {
                    Width = contentWidth,
                    Y = Pos.Bottom(panel.Subviews[panel.Subviews.Count - 1]),
                });

                height += 2;
            }

            Add(left, right);
            description = new Label(Resources.HelpDialog_ShortcutDescription)
            {
                TextColor = Application.Driver.MakeAttribute(Color.Black, Color.BrightYellow),
                X = left.X, 
                Y = maxHeight - 8,
                Width = Dim.Fill(),
                Height = 2,
            };
            Add(description);

            Height = maxHeight;
        }

        string? currentViewContext;

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            // First try local commands
            if (currentViewContext != null && 
                cmdMap.TryGetValue(((int)keyEvent.Key, currentViewContext), out var display))
            {
                description.Text = display;
                return true;
            }

            // Next try views
            if (viewMap.TryGetValue((int)keyEvent.Key, out var view))
            {
                description.Text = view.Description;
                currentViewContext = view.Context;
                return true;
            }

            // Finally try global commands
            if (cmdMap.TryGetValue(((int)keyEvent.Key, null), out display))
            {
                description.Text = display;
                return true;
            }

            currentViewContext = null;
            description.Text = Resources.HelpDialog_ShortcutDescription;
            if (keyEvent.Key == Key.Enter || keyEvent.Key == Key.Esc)
            {
                Close(true);
            }

            return true;
        }

        string GetShortcutString(int shortcut) => shortcut == -1 ? "" :
            Enum.GetName(typeof(Key), (Key)shortcut) is string keyName ?
                (Key)shortcut switch
                {
                    Key.CursorLeft => "Left",
                    Key.CursorRight => "Right",
                    _ => keyName
                } : ((char)shortcut).ToString();
    }
}