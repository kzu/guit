using System;
using System.Collections.Generic;
using Guit.Events;
using Merq;
using Terminal.Gui;

namespace Guit
{
    public class ReportStatusProgress : IDisposable
    {
        IDisposable? statusSubscription;

        ProgressDialog? progressDialog;
        MinimalProgressDialog? minimalProgressDialog;

        List<string> pendingMessages = new List<string>();

        readonly MainThread mainThread;

        public ReportStatusProgress(string title, IEventStream eventStream, MainThread mainThread)
        {
            Title = title;
            this.mainThread = mainThread;

            statusSubscription = eventStream.Of<Status>().Subscribe(OnStatus);
        }

        public string Title { get; }

        void OnStatus(Status value)
        {
            if (value.Progress > 0 && string.IsNullOrEmpty(value.NewStatus) && minimalProgressDialog == null && progressDialog == null)
            {
                minimalProgressDialog = new MinimalProgressDialog(Title);

                mainThread.Invoke(() =>
                {
                    minimalProgressDialog.Report(value.Progress);

                    Application.Run(minimalProgressDialog);
                });
            }
            if (value.Progress > 0 && !string.IsNullOrEmpty(value.NewStatus) && progressDialog == null)
            {
                if (minimalProgressDialog != null)
                {
                    // Replce the minimal dialog with the full dialog
                    minimalProgressDialog.Running = false;
                    minimalProgressDialog = null;
                }

                progressDialog = new ProgressDialog(Title);

                mainThread.Invoke(() =>
                {
                    foreach (var pendingMessage in pendingMessages)
                        progressDialog.Report(pendingMessage, value.Progress);

                    progressDialog.Report(value.NewStatus, value.Progress);

                    Application.Run(progressDialog);
                });
            }
            else if (progressDialog != null)
            {
                mainThread.Invoke(() => progressDialog.Report(value.NewStatus, value.Progress));
            }
            else if (minimalProgressDialog != null)
            {
                mainThread.Invoke(() => minimalProgressDialog.Report(value.Progress));
            }
            else if (!string.IsNullOrEmpty(value.NewStatus))
            {
                pendingMessages.Add(value.NewStatus);
            }
        }

        public void Dispose()
        {
            if (statusSubscription != null)
            {
                statusSubscription.Dispose();
                statusSubscription = null;
            }

            if (minimalProgressDialog != null)
            {
                // Auto dismiss the minimal dialog
                minimalProgressDialog.Running = false;
            }
        }
    }
}