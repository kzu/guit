namespace Guit
{
    public class PluginInfo
    {
        public PluginInfo(string id) => Id = id;

        public string Id { get; }
        public bool IsAvailable { get; set; }
        public string? Version { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }

        public override string ToString() => Title + ", v" + Version;

        public override int GetHashCode() => Id.GetHashCode();

        public override bool Equals(object obj) => obj is PluginInfo info && info.Id == Id;
    }
}
