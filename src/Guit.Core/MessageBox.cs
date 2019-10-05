using System;
using System.Linq;
using Terminal.Gui;

namespace Guit
{
    /// <summary>
    /// Message box dialog for showing messages.
    /// </summary>
    public class MessageBox : DialogBox
    {
        const int MinWidth = 60;

        public MessageBox(string title, string message)
            : base(title)
        {
            Message = message;
            Buttons = DialogBoxButton.Ok;
        }

        protected override void EndInit()
        {
            base.EndInit();

            var lines = Message
                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            var maxLineLength = lines
                .Select(x => x.Length)
                .Max();

            if (maxLineLength > MinWidth)
                Width = maxLineLength + 10;
            else
                Width = MinWidth;

            Height = lines.Length <= 10 ? lines.Length + 7 : Dim.Fill(8);

            var textView = new TextView()
            {
                Text = Message,
                ReadOnly = true,
                X = 1,
                Y = 1,
                Width = Dim.Fill(2),
                Height = Dim.Height(this) - 6
            };

            Add(textView);
        }

        public string Message { get; }
    }
}