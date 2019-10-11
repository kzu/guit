using System;
using System.Composition;
using Guit.Events;
using Merq;

namespace Guit
{
    [Shared]
    [Export]
    [Singleton]
    public class Selection 
    {
        [ImportingConstructor]
        public Selection(IEventStream eventStream) => eventStream.Of<SelectionChanged>().Subscribe(OnSelectionChanged);

        void OnSelectionChanged(SelectionChanged changed) => Current = changed?.SelectedObject;

        public object? Current { get; private set; }
    }
}
