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
                action();
            else
                Application.MainLoop.Invoke(action);
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
                return result;
            }
        }
    }
}
