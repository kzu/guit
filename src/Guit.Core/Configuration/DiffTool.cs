namespace Guit.Configuration
{
    public class DiffTool
    {
        public string? Path { get; set; }
        public string? Cmd { get; set; }
        public bool Prompt { get; set; } = false;
        public bool TrustExitCode { get; set; } = true;
    }
}