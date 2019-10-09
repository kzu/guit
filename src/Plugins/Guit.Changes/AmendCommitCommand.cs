using System.Composition;
using LibGit2Sharp;
using Merq;

namespace Guit.Plugin.Changes
{
    [Shared]
    [MenuCommand("Amend", 'a', nameof(Changes))]
    public class AmendCommitCommand : CommitCommand
    {
        [ImportingConstructor]
        public AmendCommitCommand(IEventStream eventStream, MainThread mainThread, IRepository repository, ChangesView changes)
            : base(eventStream, mainThread, repository, changes)
        {
            Amend = true;
        }
    }
}