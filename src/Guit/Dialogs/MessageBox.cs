using Terminal.Gui;

namespace Guit
{
    /// <summary>
    /// Message box dialog for showing messages.
    /// </summary>
    class MessageBox : DialogBox
    {
        public MessageBox(string title, string message)
            : base(title)
        {
            Message = message;
        }

        protected override void EndInit()
        {
            Width = 60;
            Height = 10;

            Add(new Label(string.Empty) { Y = 1, X = 1, Width = Dim.Fill(2) }, nameof(Message));

            base.EndInit();
        }

        public string Message { get; set; }
    }
}