using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;
using LibGit2Sharp;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    [Export]
    class RepositoryConfigProvider
    {
        readonly Dictionary<string, RepositoryConfig> configs = new Dictionary<string, RepositoryConfig>();

        readonly static string[] CommitsToBeIgnored = new string[] { "LEGO:", "Merge pull request #" };

        public RepositoryConfigProvider()
        {
            // TODO: Read config from git config files
            configs.Add("XamarinVS", new RepositoryConfig("origin/master", "origin/d16-4") { IgnoreCommits = CommitsToBeIgnored });
            configs.Add("xamarin-templates", new RepositoryConfig("origin/master", "origin/d16-4") { IgnoreCommits = CommitsToBeIgnored });
            configs.Add("device-manager", new RepositoryConfig("origin/master", "origin/d16-4") { IgnoreCommits = CommitsToBeIgnored });
            configs.Add("android-sdk-installer", new RepositoryConfig("origin/master", "origin/d16-4") { IgnoreCommits = CommitsToBeIgnored });
            configs.Add("clide", new RepositoryConfig("origin/master", "origin/master") { IgnoreCommits = CommitsToBeIgnored });
            configs.Add("merq", new RepositoryConfig("origin/master", "origin/master") { IgnoreCommits = CommitsToBeIgnored });
            configs.Add("vsmac-xamarin-extensions", new RepositoryConfig("origin/master", "origin/release-8.4") { IgnoreCommits = CommitsToBeIgnored });
            configs.Add("Xamarin.Messaging", new RepositoryConfig("origin/master", "origin/d16-4") { IgnoreCommits = CommitsToBeIgnored });
        }

        public RepositoryConfig? GetConfig(IRepository repository) =>
            configs.TryGetValue(repository.GetName(), out var config) ? config : default;
    }
}