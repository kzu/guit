namespace Guit
{
    /// <summary>
    /// Service to track the currently selected object.
    /// </summary>
    /// <remarks>
    /// There are two mechanisms to set the <see cref="SelectedObject"/>:
    /// <list type="bullet">
    ///     <item>Directly setting the <see cref="SelectedObject"/> property.</item>
    ///     <item>Indirectly by pushing to the <see cref="Merq.IEventStream"/> a <see cref="Events.SelectionChanged"/> event.</item>
    /// </list>
    /// <para>
    /// When changing the selection by directly setting <see cref="SelectedObject"/>, a 
    /// <see cref="Events.SelectionChanged"/> event will be pushed to the <see cref="Merq.IEventStream"/> 
    /// to signal the change to other listeners.
    /// </para>
    /// </remarks>
    public interface ISelectionService
    {
        /// <summary>
        /// Gets or sets the currently selected object. 
        /// Can be null if nothing is currently selected.
        /// </summary>
        public object? SelectedObject { get; set; }
    }
}
