using System.Collections.Generic;
using System.Composition;
using Merq;

namespace Guit
{
    [Export(typeof(ICommandBus))]
    public class CommandBus : Merq.CommandBus
    {
        [ImportingConstructor]
        public CommandBus([ImportMany] IEnumerable<ICommandHandler> handlers)
            : base(handlers)
        {
            Default = this;
        }

        public static ICommandBus Default { get; private set; }
    }
}