using System;
using System.Linq;
using System.Collections.Generic;
using System.Composition;
using LibGit2Sharp;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    class RepositoryProvider
    {
        readonly static string[] commitsToBeIgnored = new string[] { "LEGO:", "Merge pull request #" };
        readonly Lazy<List<RepositoryConfig>> repositories;

        [ImportingConstructor]
        public RepositoryProvider(IRepository root)
        {
            repositories = new Lazy<List<RepositoryConfig>>(() =>
                root.Submodules.Select(x => GetConfig(x)).ToList());
        }

        RepositoryConfig GetConfig(Submodule submodule) => GetConfig(submodule, "-Preview3");

        RepositoryConfig GetConfig(Submodule submodule, string mergeBranchSuffix) =>
            submodule.Name switch
            {
                "XamarinVS" => new RepositoryConfig(CreateRepository(submodule), "master", "d16-4", mergeBranchSuffix) { IgnoreCommits = commitsToBeIgnored },
                "debugger-vs" => new RepositoryConfig(CreateRepository(submodule), "master", "d16-4", mergeBranchSuffix) { IgnoreCommits = commitsToBeIgnored },
                "xamarin-templates" => new RepositoryConfig(CreateRepository(submodule), "master", "d16-4", mergeBranchSuffix) { IgnoreCommits = commitsToBeIgnored },
                "device-manager" => new RepositoryConfig(CreateRepository(submodule), "master", "d16-4", mergeBranchSuffix) { IgnoreCommits = commitsToBeIgnored },
                "android-sdk-installer" => new RepositoryConfig(CreateRepository(submodule), "master", "d16-4", mergeBranchSuffix) { IgnoreCommits = commitsToBeIgnored },
                "Xamarin.Messaging" => new RepositoryConfig(CreateRepository(submodule), "master", "d16-4", mergeBranchSuffix) { IgnoreCommits = commitsToBeIgnored },
                "vsmac-xamarin-extensions" => new RepositoryConfig(CreateRepository(submodule), "master", "release-8.4", "-Preview2") { IgnoreCommits = commitsToBeIgnored },
                "clide" => new RepositoryConfig(CreateRepository(submodule), "master", "master", mergeBranchSuffix) { IgnoreCommits = commitsToBeIgnored },
                "merq" => new RepositoryConfig(CreateRepository(submodule), "master", "master", mergeBranchSuffix) { IgnoreCommits = commitsToBeIgnored },
                _ => throw new InvalidOperationException(string.Format("Could not find the config for '{0}' repository", submodule.Name))
            };

        IRepository CreateRepository(Submodule submodule) => new Repository(submodule.Path);

        [Export]
        public IEnumerable<RepositoryConfig> Repositories => repositories.Value;
    }
}