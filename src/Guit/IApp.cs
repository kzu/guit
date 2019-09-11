using System.Threading;
using System.Threading.Tasks;
using Guit.Plugin;

namespace Guit
{
    /// <summary>
    /// Allows commands to run main views, which cause the current main view 
    /// to be closed and the new one showed.
    /// </summary>
    public interface IApp
    {
        /// <summary>
        /// Runs the given main view as the top-level view in the app.
        /// </summary>
        Task RunAsync(MainView view, CancellationToken cancellation = default);
    }
}