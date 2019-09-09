namespace Guit.Events
{
    public class StatusUpdated
    {
        public StatusUpdated(string newStatus) => NewStatus = newStatus;

        public string NewStatus { get; set; }

        public static implicit operator StatusUpdated(string newStatus) => new StatusUpdated(newStatus);
    }
}
