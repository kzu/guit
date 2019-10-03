using System.Threading.Tasks;

namespace Guit
{
    /// <summary>
    /// Allows commands to run main views, which cause the current main view 
    /// to be closed and the new one showed.
    /// </summary>
    public interface IApp
    {
        /// <summary>
        /// Gets the current view being shown
        /// </summary>
        ContentView CurrentView { get; }

        /// <summary>
        /// Runs the given main view as the top-level view in the app.
        /// </summary>
        Task RunAsync(ContentView view);

        /// <summary>
        /// Runs the next registered content view
        /// </summary>
        Task RunNext();

        /// <summary>
        /// Runs the previous registered content view
        /// </summary>
        Task RunPrevious();
    }
}
