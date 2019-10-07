using Terminal.Gui;

namespace Guit
{
    /// <summary>
    /// Simple input box dialog for entering a single value.
    /// </summary>
    public class InputBox : DialogBox
    {
        readonly string[] completions;

        public InputBox(string title, string message, params string[] completions)
            : base(title)
        {
            Message = message;
            this.completions = completions;
        }

        protected override void EndInit()
        {
            Width = 60;
            Height = 10;

            var messageLabel = Bind(
                new Label(Message)
                {
                    Y = 1,
                }, nameof(Message));

            var textField = Bind(new CompletionTextField(completions), nameof(Text));

            Add(new StackPanel(messageLabel, textField) { X = 1, Width = Dim.Fill(2) });

            base.EndInit();
        }

        public string? Message { get; set; }

        public string? Text { get; set; }
    }
}