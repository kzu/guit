using System;
using System.Collections.Generic;
using Guit.Events;
using Merq;
using Terminal.Gui;

namespace Guit
{
    public class ReportStatusProgress : IDisposable
    {
        IDisposable statusSubscription;
        ProgressDialog progressDialog;
        List<string> pendingMessages = new List<string>();

        public ReportStatusProgress(string title, IEventStream eventStream)
        {
            Title = title;

            statusSubscription = eventStream.Of<StatusUpdated>().Subscribe(OnStatusUpdated);
        }

        public string Title { get; }

        void OnStatusUpdated(StatusUpdated value)
        {
            if (value.Progress > 0 && progressDialog == null)
            {
                progressDialog = new ProgressDialog(Title);

                Update(progressDialog, x => Application.Run(x));

                foreach (var pendingMessage in pendingMessages)
                    Update(progressDialog, x => x.Report(pendingMessage, value.Progress));

                pendingMessages.Clear();
            }

            if (progressDialog != null)
                Update(progressDialog, x => x.Report(value.NewStatus, value.Progress));
            else
                pendingMessages.Add(value.NewStatus);
        }

        public void Dispose()
        {
            statusSubscription.Dispose();
            statusSubscription = null;
            progressDialog = null;
        }

        void Update(ProgressDialog dialog, Action<ProgressDialog> action) =>
            Application.MainLoop.Invoke(() => action(dialog));
    }
}