using System.Composition;
using Merq;

namespace DotNetGit.Events
{
    [Export(typeof(IEventStream))]
    public class EventStream : Merq.EventStream
    {
    }
}
