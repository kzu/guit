using System.Threading.Tasks;

namespace Guit
{
    /// <summary>
    /// Allows commands to run main views, which cause the current main view 
    /// to be closed and the new one showed.
    /// </summary>
    public interface IShell
    {
        /// <summary>
        /// Gets the current view being shown
        /// </summary>
        ContentView? CurrentView { get; }

        /// <summary>
        /// Shuts down the main shell UI.
        /// </summary>
        void Shutdown();

        /// <summary>
        /// Runs the given main view as the top-level view in the app.
        /// </summary>
        Task RunAsync(ContentView view);

        /// <summary>
        /// Runs the given main view as the top-level view in the app.
        /// </summary>
        Task RunAsync(string contentViewId);

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
