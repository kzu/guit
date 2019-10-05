using System.Composition;
using System.IO;
using LibGit2Sharp;

namespace Guit
{
    [Shared]
    public class RepositoryProvider
    {
        [ImportingConstructor]
        public RepositoryProvider() : this(Directory.GetCurrentDirectory()) { }

        public RepositoryProvider(string currentDir)
        {
            string? repoRoot = currentDir;
            while (!string.IsNullOrEmpty(repoRoot) && !Repository.IsValid(repoRoot))
            {
                repoRoot = Directory.GetParent(repoRoot)?.FullName;
            }

            Repository = new Repository(repoRoot ?? currentDir);
            var version = Repository.Config.Get<string>("guit.version")?.Value;
            if (string.IsNullOrEmpty(version))
            {
                // Perform initial configuration
                Repository.Config.Set("guit.version", ThisAssembly.Metadata.Version);
            }
            else if (version != ThisAssembly.Metadata.Version)
            {
                // Perform config migrations if needed
                Repository.Config.Set("guit.version", ThisAssembly.Metadata.Version);
            }
        }

        [Export(typeof(ISingleton))]
        [Export]
        public Repository Repository { get; }

        [Export(typeof(ISingleton))]
        [Export]
        public IRepository IRepository => Repository;
    }
}
