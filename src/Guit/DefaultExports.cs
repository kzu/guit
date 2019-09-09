using System.Composition;
using System.IO;
using System.Threading;
using LibGit2Sharp;
using Microsoft.VisualStudio.Threading;

namespace Guit
{
    [Shared]
    public class DefaultExports
    {
        [Export]
        public Repository Repository { get; } = new Repository(Directory.GetCurrentDirectory());

        [Export]
        public JoinableTaskFactory TaskFactory { get; } = new JoinableTaskFactory(new JoinableTaskContext(synchronizationContext: SynchronizationContext.Current));

        [Export]
        public JoinableTaskContext TaskContext => TaskFactory.Context;
    }
}
