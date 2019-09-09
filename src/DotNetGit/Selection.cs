using System;
using System.Composition;
using DotNetGit.Events;
using Merq;

namespace DotNetGit
{
    [Shared]
    [Export]
    [Export(typeof(ISingleton))]
    public class Selection : ISingleton
    {
        [ImportingConstructor]
        public Selection(IEventStream eventStream) => eventStream.Of<SelectionChanged>().Subscribe(OnSelectionChanged);

        void OnSelectionChanged(SelectionChanged changed) => Current = changed?.SelectedObject;

        public object Current { get; private set; }
    }
}
