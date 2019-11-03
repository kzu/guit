namespace Guit.Configuration
{
    /// <summary>
    /// Strong-typed configuration for a diff tool.
    /// </summary>
    public class DiffTool
    {
        public string? Path { get; set; }
        public string? Cmd { get; set; }
        public bool Prompt { get; set; } = false;
        public bool TrustExitCode { get; set; } = true;
    }
}