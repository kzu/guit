using System.Collections.Generic;
using System.Composition;
using Merq;

namespace DotNetGit
{
    [Shared]
    [Export(typeof(IEventStream))]
    public class EventStream : Merq.EventStream
    {
        [ImportingConstructor]
        public EventStream([ImportMany("IObservable")] IEnumerable<object> observables)
            : base(observables)
        {
            Default = this;
        }

        public static IEventStream Default { get; private set; }
    }
}
