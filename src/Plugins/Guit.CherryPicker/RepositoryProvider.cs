using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    class RepositoryProvider
    {
        readonly Lazy<Dictionary<string, ReleaseConfig>> repositories = new Lazy<Dictionary<string, ReleaseConfig>>();

        [ImportingConstructor]
        public RepositoryProvider(IRepository root)
        {
            repositories = new Lazy<Dictionary<string, ReleaseConfig>>(() => ReadConfig(root));
        }

        Dictionary<string, ReleaseConfig> ReadConfig(IRepository root)
        {
            var configs = new Dictionary<string, ReleaseConfig>();
            var configFile = Path.Combine(root.Info.WorkingDirectory, ".releaseconfig");

            var ignoredCommits = Enumerable.Empty<string>();
            if (File.Exists(Constants.NoReleaseFile))
            {
                ignoredCommits = File
                    .ReadAllLines(Constants.NoReleaseFile)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Select(x => x.Split('\t').LastOrDefault())
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();
            }

            if (File.Exists(configFile))
            {
                var config = LibGit2Sharp.Configuration.BuildFrom(configFile);
                var defaultSource = config.GetValueOrDefault("repository.source", default(string));
                var defaultTarget = config.GetValueOrDefault("repository.target", default(string));

                var defaultIgnores = config
                    .OfType<ConfigurationEntry<string>>()
                    .Where(x => x.Key == "repository.ignore")
                    .Select(x => x.Value)
                    .ToArray();

                foreach (var submodule in root.Submodules)
                {
                    // TODO: fail if a submodule has no configuration?
                    var source = config.GetValueOrDefault("repository", submodule.Name, "source", defaultSource);
                    var target = config.GetValueOrDefault("repository", submodule.Name, "target", defaultTarget);

                    var ignores = config
                        .OfType<ConfigurationEntry<string>>()
                        .Where(x => x.Key == "repository." + submodule.Name + ".ignore")
                        .Select(x => x.Value)
                        .Concat(defaultIgnores)
                        .Concat(ignoredCommits)
                        .ToArray();

                    if (source != null && target != null)
                        configs.Add(submodule.Name, new ReleaseConfig(CreateRepository(submodule), source, target) { IgnoreCommits = ignores });
                }
            }

            return configs;
        }

        IRepository CreateRepository(Submodule submodule) => new Repository(submodule.Path);

        [Export]
        public IEnumerable<ReleaseConfig> Repositories => repositories.Value.Values;
    }
}