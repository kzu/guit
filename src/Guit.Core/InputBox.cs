using Terminal.Gui;

namespace Guit
{
    /// <summary>
    /// Simple input box dialog for entering a single value.
    /// </summary>
    public class InputBox : DialogBox
    {
        public InputBox(string title, string message)
            : base(title)
        {
            Message = message;
        }

        protected override void EndInit()
        {
            Width = 60;
            Height = 10;

            var messageLabel = Bind(
                new Label(string.Empty)
                {
                    Y = 1,
                    Width = Dim.Fill(2)
                }, nameof(Message));

            var textField = Bind(new TextField(string.Empty), nameof(Text));

            InitialFocusedView = textField;

            Add(new StackPanel(messageLabel, textField));

            base.EndInit();
        }

        public string? Message { get; set; }

        public string? Text { get; set; }
    }
}