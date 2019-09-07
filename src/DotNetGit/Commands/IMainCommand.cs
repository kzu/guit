using System.Threading;
using System.Threading.Tasks;

namespace DotNetGit.Commands
{
    public interface IMainCommand
    {
        Task ExecuteAsync(CancellationToken cancellation);
    }
}