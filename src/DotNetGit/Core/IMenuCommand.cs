using System.Threading;
using System.Threading.Tasks;

namespace DotNetGit
{
    public interface IMenuCommand
    {
        Task ExecuteAsync(CancellationToken cancellation);
    }
}