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

        public static Status Create(string status, params string[] args) => Create(default, status, args);

        public static Status Create(float progress, string status, params string[] args) =>
            new Status(args != null ? string.Format(status, args) : status, progress);

        public static Status Start(string status, params string[] args) => Create(0.1f, status, args);

        public static Status Finish(string status, params string[] args) => Create(1, status, args);

        public static Status Succeeded() => Create(1, "Succeeded!");

        public static Status Failed() => Create(1, "Failed");
    }
}