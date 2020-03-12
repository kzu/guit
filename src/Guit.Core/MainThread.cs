using System;
using System.Composition;
using System.Threading;
using Terminal.Gui;

namespace Guit
{
    /// <summary>
    /// Ensures the execution of an action happens on the UI thread.
    /// </summary>
    [Export]
    [Shared]
    public class MainThread
    {
        int mainThreadId;

        public MainThread() => mainThreadId = Thread.CurrentThread.ManagedThreadId;

        public void Invoke(Action action)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
            {
                action();
            }
            else
            {
                var ev = new ManualResetEventSlim();

                Application.MainLoop.Invoke(() =>
                {
                    // In order to avoid deadlocks we should wait until the action is scheduled
                    // and it's about to start running before returning control to the caller
                    ev.Set();

                    action();
                });

                ev.Wait();
            }
        }

        public T Invoke<T>(Func<T> function)
        {
            if (Thread.CurrentThread.ManagedThreadId == mainThreadId)
            {
                return function();
            }
            else
            {
                // Run and wait for a result.
                var ev = new ManualResetEventSlim();
                T result = default;

                Application.MainLoop.Invoke(() =>
                {
                    result = function();
                    ev.Set();
                });

                ev.Wait();
#pragma warning disable CS8603 // Possible null reference return.
                return result;
#pragma warning restore CS8603 // Possible null reference return.
            }
        }
    }
}
