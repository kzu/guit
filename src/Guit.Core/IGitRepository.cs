using System;
using System.Collections.Generic;
using Merq;
using LibGit2Sharp.Handlers;
using LibGit2Sharp;

namespace Guit
{
    public interface IGitRepository : IRepository
    {
        public string GetFullPath(string filePath);

        public void RevertFileChanges(params string[] filePaths);

        public IEnumerable<string> GetBranchNames();

        public IEnumerable<string> GetRemoteNames();

        public string GetDefaultRemoteName(string defaultRemoteName = "origin");

        public void UpdateSubmodules(bool recursive = true, IEventStream? eventStream = null);

        public void Fetch(CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false);

        public void Fetch(string remoteName, CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false);

        public void Fetch(Remote remote, CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false);

        public void Fetch(IEnumerable<Remote> remotes, CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false);

        public Branch CreateBranch(string branchName);

        public void Checkout(Branch branch);

        public void Stage(string filepath);

        public void Remove(string filepath);

        public IEnumerable<Commit> GetCommitsToBeRebased(Branch branch);

        public string GetRepoUrl();

        public void OpenUrl(Commit commit);
    }
}