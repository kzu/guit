using System.Composition;
using LibGit2Sharp;
using Merq;

namespace Guit.Plugin.Changes
{
    [Shared]
    [ChangesCommand(WellKnownCommands.Changes.Amend, 'a')]
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