using System.Composition;
using Merq;
using Terminal.Gui;
using System;
using LibGit2Sharp;
using Guit.Events;

namespace Guit
{
    [Shared]
    [Export]
    public class StatusBar : View
    {
        readonly Repository repository;
        readonly Label status;
        readonly Label clock;
        object clearMessageToken;

        [ImportingConstructor]
        public StatusBar(IEventStream eventStream, Repository repository)
        {
            this.repository = repository;

            CanFocus = false;
            Height = 1;
            ColorScheme = new ColorScheme
            {
                Normal = Terminal.Gui.Attribute.Make(Color.White, Color.Black)
            };

            status = new Label("Ready")
            {
                // Initially, we can only use the Frame of the top view, since Width is null at this point.
                // See LayoutSubviews below
                Width = Application.Top.Frame.Width - ClockText.Length
            };
            clock = new Label(ClockText)
            {
                Width = ClockText.Length,
                X = Pos.Right(status)
            };

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
            eventStream.Of<StatusUpdated>().Subscribe(OnStatusUpdated);
        }

        public override void LayoutSubviews()
        {
            // At this point, Application.Current has a Width. For some reason, this same 
            // code in the constructor doesn't work.
            status.Width = Application.Current.Width - clock.Width;
            base.LayoutSubviews();
        }

        string ClockText => repository.Config.Get<string>("user.name").Value + " (" + repository.Config.Get<string>("user.email").Value + ") | " + DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();

        void OnStatusUpdated(StatusUpdated value)
        {
            if (clearMessageToken != null)
                Application.MainLoop.RemoveTimeout(clearMessageToken);

            status.Text = value.NewStatus;
            clearMessageToken = Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(5), _ =>
            {
                status.Text = "Ready";
                return false;
            });
        }
    }
}