using System.Composition;
using Merq;
using Terminal.Gui;
using System;

namespace DotNetGit
{
    [Shared]
    [Export]
    public class StatusBar : View
    {
        object clearMessageToken;

        Label status = new Label("Ready")
        {
            // Initially, we can only use the Frame of the top view, since Width is null at this point.
            // See LayoutSubviews below
            Width = Application.Top.Frame.Width - ClockText.Length,
        };
        Label clock = new Label(ClockText)
        {
            Width = ClockText.Length,
        };

        [ImportingConstructor]
        public StatusBar(IEventStream eventStream)
        {
            CanFocus = false;
            Height = 1;

            ColorScheme = Colors.Menu;
            clock.X = Pos.Right(status);
            
            // To see the width of the clock itself, uncomment the following line
            //clock.ColorScheme = Colors.Error;

            Add(status, clock);

            Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(1), _ =>
            {
                clock.Text = ClockText;
                return true;
            });

            // Pushing simple strings to the event stream will cause them to update the 
            // status message.
            eventStream.Of<string>().Subscribe(OnValue);
        }

        public override void LayoutSubviews()
        {
            // At this point, Application.Current has a Width. For some reason, this same 
            // code in the constructor doesn't work.
            status.Width = Application.Current.Width - clock.Width;

            base.LayoutSubviews();
        }

        static string ClockText => DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString();

        void OnValue(string value)
        {
            if (clearMessageToken != null)
                Application.MainLoop.RemoveTimeout(clearMessageToken);

            status.Text = value;
            clearMessageToken = Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(5), _ =>
            {
                status.Text = "Ready";
                return false;
            });
        }
    }
}