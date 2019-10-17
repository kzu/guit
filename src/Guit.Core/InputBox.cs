using Terminal.Gui;

namespace Guit
{
    /// <summary>
    /// Simple input box dialog for entering a single value.
    /// </summary>
    public class InputBox : DialogBox
    {
        const int MinWidth = 60;

        readonly string[] completions;

        public InputBox(string title, string message, params string[] completions)
            : base(title)
        {
            Message = message;
            this.completions = completions;
        }

        protected override void EndInit()
        {
            Height = 10;

            Width = Message.Length > MinWidth ? Message.Length + 10 : MinWidth;

            var messageLabel = Bind(
                new Label(Message)
                {
                    Y = 1,
                }, nameof(Message));

            var textField = Bind(new CompletionTextField(completions), nameof(Text));

            Add(new StackPanel(messageLabel, textField) { X = 1, Width = Dim.Fill(2) });

            base.EndInit();
        }

        public string Message { get; set; }

        public string? Text { get; set; }
    }
}