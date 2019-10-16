using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Guit.Events;
using LibGit2Sharp;
using Merq;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    [MenuCommand("Ignore", 'i', nameof(Releaseator))]
    class IgnoreCommand : IMenuCommand
    {
        readonly IEventStream eventStream;
        readonly MainThread mainThread;
        readonly IRepository repository;
        readonly ReleaseatorView view;

        [ImportingConstructor]
        public IgnoreCommand(IEventStream eventStream, MainThread mainThread, IRepository repository, ReleaseatorView view)
        {
            this.eventStream = eventStream;
            this.mainThread = mainThread;
            this.repository = repository;
            this.view = view;
        }

        public Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default)
        {
            return Task.CompletedTask;
        }
    }
}