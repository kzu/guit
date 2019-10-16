using System.Threading;
using System.Threading.Tasks;

namespace Guit
{
    public interface IAfterExecuteCallback
    {
        Task AfterExecuteAsync(CancellationToken cancellation);
    }
}