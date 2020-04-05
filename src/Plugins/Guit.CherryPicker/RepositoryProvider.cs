using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using LibGit2Sharp;

namespace Guit.Plugin.CherryPicker
{
    [Shared]
    class RepositoryProvider
    {
        readonly Lazy<List<CherryPickConfig>> repositories = new Lazy<List<CherryPickConfig>>();

        [ImportingConstructor]
        public RepositoryProvider(IGitRepository root)
        {
            repositories = new Lazy<List<CherryPickConfig>>(() => ReadConfig(root));
        }

        List<CherryPickConfig> ReadConfig(IGitRepository root)
        {
            var configs = new List<CherryPickConfig>();

            var releaseConfigFile = Path.Combine(root.Info.WorkingDirectory, ".releaseconfig");

            if (File.Exists(releaseConfigFile))
            {
                var releaseConfig = LibGit2Sharp.Configuration.BuildFrom(releaseConfigFile);
                var defaultSource = releaseConfig.GetValueOrDefault("repository.source", default(string));
                var defaultTarget = releaseConfig.GetValueOrDefault("repository.target", default(string));

                foreach (var submodule in root.Submodules)
                {
                    var repository = CreateRepository(submodule);

                    // TODO: fail if a submodule has no configuration?
                    var source = releaseConfig.GetValueOrDefault("repository", repository.GetName(), "source", defaultSource);
                    var target = releaseConfig.GetValueOrDefault("repository", repository.GetName(), "target", defaultTarget);

                    if (source != null && target != null)
                        configs.Add(new CherryPickConfig(repository, source, target) { SyncTargetBranch = true });
                }
            }
            else
            {
                configs.Add(new CherryPickConfig(root));
            }

            if (File.Exists(Constants.NoCherryPick))
            {
                var noCherryPickConfig = LibGit2Sharp.Configuration.BuildFrom(Constants.NoCherryPick);

                foreach (var config in configs)
                {
                    config.IgnoreCommits = noCherryPickConfig
                        .OfType<ConfigurationEntry<string>>()
                        .Where(x => x.Key == "repository.ignore" || x.Key == $"repository.{config.Repository.GetName()}.ignore")
                        .Select(x => x.Value)
                        .ToList();
                }
            }

            return configs;
        }

        IGitRepository CreateRepository(Submodule submodule) => new GitRepository(submodule.Path);

        [Export]
        public IEnumerable<CherryPickConfig> Repositories => repositories.Value;
    }
}