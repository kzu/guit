using System.Threading;
using System.Threading.Tasks;

namespace Guit
{
    public interface IMenuCommand
    {
        Task ExecuteAsync(CancellationToken cancellation);
    }
}