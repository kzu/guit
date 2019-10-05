using Terminal.Gui;

namespace Guit
{
    /// <summary>
    /// Message box dialog for showing messages.
    /// </summary>
    public class MessageBox : DialogBox
    {
        public MessageBox(string title, string message)
            : base(title)
        {
            Message = message;
            Buttons = DialogBoxButton.Ok;
        }

        protected override void EndInit()
        {
            base.EndInit();

            Width = 60;
            Height = 10;

            Add(new Label(string.Empty) { Y = 1, X = 1, Width = Dim.Fill(2) }, nameof(Message));
        }

        public string Message { get; set; }
    }
}