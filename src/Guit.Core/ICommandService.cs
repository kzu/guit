using System.Threading;
using System.Threading.Tasks;

namespace Guit
{
    public interface ICommandService
    {
        Task RunAsync(string commandId, object? parameter = null, CancellationToken cancellation = default);
    }
}
