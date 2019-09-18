using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Guit
{
    /// <summary>
    /// Allows executing work on different threading contexts.
    /// </summary>
    [Export]
    [Shared]
    public class ThreadContext
    {
        /// <summary>
        /// The default thread context, initialized at application start.
        /// </summary>
        public static ThreadContext Default { get; private set; }

        /// <summary>
        /// Initializes the thread context and captures the current <see cref="Thread.ManagedThreadId"/>.
        /// </summary>
        public ThreadContext()
        {
            MainThread = new MainThreadInvoker(Thread.CurrentThread.ManagedThreadId);
            Background = new BackgroundThreadInvoker(Thread.CurrentThread.ManagedThreadId);

            Default = this;
        }

        /// <summary>
        /// Allows executing work on the main thread.
        /// </summary>
        public IInvoker MainThread { get; private set; }

        /// <summary>
        /// Ensures the work to execute runs on a background thread.
        /// </summary>
        public IInvoker Background { get; private set; }

        class MainThreadInvoker : IInvoker
        {
            readonly int mainThreadId;

            public MainThreadInvoker(int mainThreadId) => this.mainThreadId = mainThreadId;

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

        class BackgroundThreadInvoker : IInvoker
        {
            readonly int mainThreadId;

            public BackgroundThreadInvoker(int mainThreadId) => this.mainThreadId = mainThreadId;

            public void Invoke(Action action)
            {
                if (Thread.CurrentThread.ManagedThreadId != mainThreadId)
                    action();
                else
                    Task.Run(action);
            }

            public T Invoke<T>(Func<T> function)
            {
                if (Thread.CurrentThread.ManagedThreadId != mainThreadId)
                {
                    return function();
                }
                else
                {
                    // TODO: this might deadlock
                    return Task.Run(function).Result;
                }
            }
        }

        public interface IInvoker
        {
            void Invoke(Action action);
            T Invoke<T>(Func<T> function);
        }
    }
}
    