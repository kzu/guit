using System.Collections.Generic;
using System.Composition;
using Merq;

namespace DotNetGit.Commands
{
    [Export(typeof(ICommandBus))]
    public class CommandBus : Merq.CommandBus
    {
        [ImportingConstructor]
        public CommandBus([ImportMany] IEnumerable<ICommandHandler> handlers)
            : base(handlers)
        {
        }
    }
}