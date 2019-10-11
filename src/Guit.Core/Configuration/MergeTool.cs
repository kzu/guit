namespace Guit.Configuration
{
    public class MergeTool
    {
        public string? Path { get; set; }
        public string? Cmd { get; set; }
        public bool TrustExitCode { get; set; } = true;
        public bool KeepBackup { get; set; }
    }
}