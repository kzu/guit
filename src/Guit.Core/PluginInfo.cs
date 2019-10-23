namespace Guit
{
    public class PluginInfo
    {
        public PluginInfo(string spec) => Spec = spec;

        public string Spec { get; set; }

        public string Id { get; set; }
        public bool IsAvailable { get; set; }
        public string? Version { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public override string ToString() => Version == null ? (Title ?? Spec) : Title + ", v" + Version;

        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object obj) => obj is PluginInfo info && info.Id == Id;
    }
}
