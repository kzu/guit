using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Terminal.Gui;
using System.Data;
using System.Threading;
using LibGit2Sharp;
using Merq;
using Guit.Events;

namespace Guit
{
    [Export]
    [Export(typeof(IShell))]
    [Shared]
    class Shell : Toplevel, IShell
    {
        readonly ConcurrentDictionary<ContentView, string?> contexts = new ConcurrentDictionary<ContentView, string?>();
        readonly ConcurrentDictionary<ContentView, ShellWindow> shellWindows = new ConcurrentDictionary<ContentView, ShellWindow>();
        readonly ConcurrentDictionary<ContentView, ManualResetEventSlim> runningViews = new ConcurrentDictionary<ContentView, ManualResetEventSlim>();

        readonly Lazy<ContentView, MenuCommandMetadata> defaultView;
        readonly IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views;

        readonly MainThread mainThread;
        readonly Lazy<CommandService> commandService;
        readonly IRepository repository;

        [ImportingConstructor]
        public Shell(
            [ImportMany] IEnumerable<Lazy<ContentView, MenuCommandMetadata>> views,
            MainThread mainThread,
            Lazy<CommandService> commandService,
            IEventStream eventStream,
            IRepository repository)
        {
            this.views = views
                .OrderBy(x => x.Metadata.Order)
                .ThenBy(x => x.Metadata.Key);

            defaultView = this.views.First();

            this.mainThread = mainThread;
            this.commandService = commandService;
            this.repository = repository;

            eventStream.Of<BranchChanged>().Subscribe(x =>
                mainThread.Invoke(() =>
                {
                    if (CurrentView != null && shellWindows.TryGetValue(CurrentView, out var shellWindow))
                        shellWindow.Refresh();
                }));
        }

        public ContentView? CurrentView { get; private set; }

        public override bool ProcessHotKey(KeyEvent keyEvent)
        {
            if (Application.Current is ShellWindow shellWindow)
                commandService.Value.RunAsync(keyEvent.KeyValue, GetContext(shellWindow.Content));

            return base.ProcessHotKey(keyEvent);
        }

        string? GetContext(ContentView? view) =>
            view != null ? contexts.GetOrAdd(view, x => x.GetType().GetCustomAttribute<ContentViewAttribute>()?.Context) : default;

        // Run the main window as soon as the app is presented.
        public override void WillPresent() => RunAsync(defaultView.Value);

        ManualResetEventSlim exitEvent = new ManualResetEventSlim();

        public void Run()
        {
            RunAsync(defaultView.Value);
            exitEvent.Wait();
        }

        public Task RunAsync(string contentViewId)
        {
            var view = views.FirstOrDefault(x => x.Metadata.Id == contentViewId);

            if (view is null)
                throw new InvalidOperationException(string.Format("View {0} not found", contentViewId));

            return RunAsync(view.Value);
        }

        public Task RunAsync(ContentView view)
        {
            if (Application.Current is ShellWindow shellWindow)
            {
                // NOTE: we don't set the event at this point, since we 
                // want the running to actually be stopped by the mainloop
                // continuing from the previous Application.Run() for that 
                // window, at which point, the event will be set when we know 
                // for sure the mainloop for that window is no longer running.
                // See bellow.
                Application.MainLoop.AddTimeout(TimeSpan.Zero, _ => shellWindow.Running = false);
                if (runningViews.TryGetValue(shellWindow.Content, out var running))
                    running.Wait();
            }

            mainThread.Invoke(() =>
            {
                CurrentView = view;
                view.Refresh();

                var window = shellWindows.GetOrAdd(view, key =>
                    new ShellWindow(
                        key.Title,
                        key,
                        Tuple.Create(0, 0, 0, 1),
                        new CommandBar(commandService.Value, GetContext(key)),
                        new RepoStatus(repository)
                        ));

                var running = runningViews.GetOrAdd(view, _ => new ManualResetEventSlim());

                running.Reset();
                Application.Run(window);
                running.Set();
            });

            return Task.CompletedTask;
        }

        public async Task RunNext()
        {
            var viewList = views.ToList();

            var currentViewWithMetadata = viewList.FirstOrDefault(x => x.Metadata.Context == GetContext(CurrentView));

            var targetIndex = viewList.IndexOf(currentViewWithMetadata) + 1;
            if (targetIndex >= viewList.Count)
                targetIndex = 0;

            await RunAsync(viewList[targetIndex].Value);
        }

        public async Task RunPrevious()
        {
            var viewList = views.ToList();

            var currentViewWithMetadata = viewList.FirstOrDefault(x => x.Metadata.Context == GetContext(CurrentView));

            var targetIndex = viewList.IndexOf(currentViewWithMetadata) - 1;
            if (targetIndex < 0)
                targetIndex = viewList.Count - 1;

            await RunAsync(viewList[targetIndex].Value);
        }

        public void Shutdown()
        {
            Application.MainLoop.AddTimeout(TimeSpan.Zero, _ =>
            {
                foreach (var window in shellWindows.Values)
                {
                    window.Running = false;
                }

                return false;
            });

            var handles = runningViews.Values.Select(x => x.WaitHandle).ToArray();
            while (!WaitHandle.WaitAll(handles, 100))
            {
                Thread.Sleep(50);
            }

            exitEvent.Set();

            Application.MainLoop.AddTimeout(TimeSpan.Zero, _ => Running = false);
        }
    }
}
