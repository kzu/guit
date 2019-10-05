namespace Guit.Events
{
    public class Status
    {
        public Status(
            string? newStatus,
            float progress = default,
            StatusImportance importance = StatusImportance.Normal)
        {
            NewStatus = newStatus;
            Progress = progress;
            Importance = importance;
        }

        public string? NewStatus { get; set; }

        public float Progress { get; set; }

        public StatusImportance Importance { get; set; }


        public static implicit operator Status(string? newStatus) => new Status(newStatus);

        public static implicit operator Status(float progress) => new Status(null, progress);
    }
}