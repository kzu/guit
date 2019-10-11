using System.IO;
using LibGit2Sharp;

namespace Guit.Events
{
    public class SelectionChanged
    {
        public SelectionChanged(object selectedObject) => SelectedObject = selectedObject;

        public object SelectedObject { get; private set; }

        public static implicit operator SelectionChanged(string selectedFile) => new SelectionChanged(selectedFile);

        public static implicit operator SelectionChanged(FileInfo selectedFile) => new SelectionChanged(selectedFile);

        public static implicit operator SelectionChanged(StatusEntry selectedStatus) => new SelectionChanged(selectedStatus);

        public static implicit operator SelectionChanged(Conflict selectedConflict) => new SelectionChanged(selectedConflict);
    }
}