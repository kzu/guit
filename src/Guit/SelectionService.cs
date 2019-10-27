using System;
using System.Composition;
using Guit.Events;
using Merq;

namespace Guit
{
    [Shared]
    [Export(typeof(ISelectionService))]
    [Singleton]
    class SelectionService : ISelectionService
    {
        readonly IEventStream eventStream;
        object? current;

        [ImportingConstructor]
        public SelectionService(IEventStream eventStream)
        {
            this.eventStream = eventStream;
            eventStream.Of<SelectionChanged>().Subscribe(OnSelectionChanged);
        }

        void OnSelectionChanged(SelectionChanged changed) => current = changed?.SelectedObject;

        public object? SelectedObject 
        {
            get => current;
            set
            {
                current = value;
                eventStream.Push(new SelectionChanged(value));    
            }
        }
    }
}
