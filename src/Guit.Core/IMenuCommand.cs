using System.Threading;
using System.Threading.Tasks;

namespace Guit
{
    public interface IMenuCommand
    {
        Task ExecuteAsync(object? parameter = null, CancellationToken cancellation = default);
    }
}