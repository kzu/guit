using System.IO;
using LibGit2Sharp;

namespace Guit.Events
{
    /// <summary>
    /// Allows modifying (and notifying) that the current selection has 
    /// changed. This event also sets the <see cref="ISelectionService.SelectedObject"/>.
    /// </summary>
    public class SelectionChanged
    {
        /// <summary>
        /// Initializes the event with the given selected object;
        /// </summary>
        public SelectionChanged(object? selectedObject) => SelectedObject = selectedObject;

        /// <summary>
        /// The selected object.
        /// </summary>
        public object? SelectedObject { get; private set; }

        /// <summary>
        /// Implicit conversion from string to <see cref="SelectionChanged"/>.
        /// </summary>
        public static implicit operator SelectionChanged(string selectedFile) => new SelectionChanged(selectedFile);

        /// <summary>
        /// Implicit conversion from <see cref="FileInfo"/> to <see cref="SelectionChanged"/>.
        /// </summary>
        public static implicit operator SelectionChanged(FileInfo selectedFile) => new SelectionChanged(selectedFile);

        /// <summary>
        /// Implicit conversion from <see cref="StatusEntry"/> to <see cref="SelectionChanged"/>.
        /// </summary>
        public static implicit operator SelectionChanged(StatusEntry selectedStatus) => new SelectionChanged(selectedStatus);

        /// <summary>
        /// Implicit conversion from <see cref="Conflict"/> to <see cref="SelectionChanged"/>.
        /// </summary>
        public static implicit operator SelectionChanged(Conflict selectedConflict) => new SelectionChanged(selectedConflict);
    }
}