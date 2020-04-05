using System;
using System.Collections.Generic;
using System.Linq;
using Merq;
using Guit.Events;
using System.IO;
using Git = LibGit2Sharp.Commands;
using LibGit2Sharp.Handlers;
using System.Diagnostics;
using LibGit2Sharp;

namespace Guit
{
    public class GitRepository : IGitRepository
    {
        const string GitSuffix = ".git";

        readonly IRepository repository;

        public GitRepository(string path)
            : this(new Repository(path))
        { }

        public GitRepository(IRepository repository)
        {
            this.repository = repository;
        }

        public Branch Head => repository.Head;

        public LibGit2Sharp.Configuration Config => repository.Config;

        public Index Index => repository.Index;

        public ReferenceCollection Refs => repository.Refs;

        public IQueryableCommitLog Commits => repository.Commits;

        public BranchCollection Branches => repository.Branches;

        public TagCollection Tags => repository.Tags;

        public RepositoryInformation Info => repository.Info;

        public Diff Diff => repository.Diff;

        public ObjectDatabase ObjectDatabase => repository.ObjectDatabase;

        public NoteCollection Notes => repository.Notes;

        public SubmoduleCollection Submodules => repository.Submodules;

        public WorktreeCollection Worktrees => repository.Worktrees;

        public Rebase Rebase => repository.Rebase;

        public Ignore Ignore => repository.Ignore;

        public Network Network => repository.Network;

        public StashCollection Stashes => repository.Stashes;

        public string GetFullPath(string filePath) =>
            Path.IsPathRooted(filePath) ? Path.GetFullPath(filePath) :
            Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, filePath));

        public void RevertFileChanges(params string[] filePaths) =>
            repository.CheckoutPaths(
                repository.Head.FriendlyName,
                filePaths,
                new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });

        public IEnumerable<string> GetBranchNames() =>
            repository
                .Branches
                .Select(x => x.GetName())
                .Distinct()
                .OrderBy(x => x);

        public IEnumerable<string> GetRemoteNames() =>
            repository
                .Network
                .Remotes
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x);

        public string GetDefaultRemoteName(string defaultRemoteName = "origin") =>
            GetRemoteNames().Contains(defaultRemoteName) ? defaultRemoteName : GetRemoteNames().FirstOrDefault();

        public void UpdateSubmodules(bool recursive = true, IEventStream? eventStream = null)
        {
            foreach (var submodule in repository.Submodules)
            {
                eventStream?.Push(Status.Create("Submodule update {0}", submodule.Name));

                try
                {
                    repository.Submodules.Update(submodule.Name, new SubmoduleUpdateOptions() { Init = true });

                    if (recursive)
                    {
                        using (var subRepository = new GitRepository(Path.Combine(repository.Info.WorkingDirectory, submodule.Path)))
                            subRepository.UpdateSubmodules(eventStream: eventStream);
                    }
                }
                catch
                {
                    eventStream?.Push(Status.Create("Failed to update submodule {0}", submodule.Name));
                }
            }
        }

        public void Fetch(CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false) =>
            Fetch(repository.Network.Remotes, credentials, eventStream, prune);

        public void Fetch(string remoteName, CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false)
        {
            if (repository.Network.Remotes.FirstOrDefault(x => x.Name == remoteName) is Remote remote)
                Fetch(remote, credentials, eventStream, prune);
        }

        public void Fetch(Remote remote, CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false) =>
            Fetch(new Remote[] { remote }, credentials, eventStream, prune);

        public void Fetch(IEnumerable<Remote> remotes, CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false)
        {
            foreach (var remote in remotes)
            {
                Git.Fetch(
                    (Repository)repository,
                    remote.Name,
                    remote.FetchRefSpecs.Select(x => x.Specification), new FetchOptions
                    {
                        Prune = prune,
                        CredentialsProvider = credentials,
                        OnProgress = serverProgressOutput =>
                        {
                            eventStream?.Push(new Status(serverProgressOutput));
                            return true;
                        },
                        OnTransferProgress = progress =>
                        {
                            eventStream?.Push(new Status($"Received {progress.ReceivedObjects} of {progress.TotalObjects}", progress.ReceivedObjects / (float)progress.TotalObjects));
                            return true;
                        }
                    }, string.Empty);
            }
        }

        public void Checkout(Branch branch) =>
            Git.Checkout(repository, branch);

        public void Stage(string filepath) =>
            Git.Stage(repository, filepath);

        public void Remove(string filepath) =>
            Git.Remove(repository, filepath);

        public IEnumerable<Commit> GetCommitsToBeRebased(Branch branch)
        {
            foreach (var commit in repository.Commits)
            {
                if (!branch.Commits.Contains(commit))
                    yield return commit;
                else
                    break;
            }
        }

        public string GetRepoUrl()
        {
            var repoUrl = repository.Config.GetValueOrDefault<string>("remote.origin.url");
            if (repoUrl.EndsWith(GitSuffix))
                repoUrl = repoUrl.Remove(repoUrl.Length - GitSuffix.Length);

            return repoUrl;
        }

        public void OpenUrl(Commit commit) =>
            Process.Start("cmd", $"/c start {GetRepoUrl()}/commit/{commit.Sha}");

        public void Checkout(Tree tree, IEnumerable<string> paths, CheckoutOptions opts) =>
            repository.Checkout(tree, paths, opts);

        public void CheckoutPaths(string committishOrBranchSpec, IEnumerable<string> paths, CheckoutOptions checkoutOptions) =>
            repository.CheckoutPaths(committishOrBranchSpec, paths, checkoutOptions);

        public GitObject Lookup(ObjectId id) =>
            repository.Lookup(id);

        public GitObject Lookup(string objectish) =>
            repository.Lookup(objectish);

        public GitObject Lookup(ObjectId id, ObjectType type) =>
            repository.Lookup(id, type);

        public GitObject Lookup(string objectish, ObjectType type) =>
            repository.Lookup(objectish, type);

        public Commit Commit(string message, Signature author, Signature committer, CommitOptions options) =>
            repository.Commit(message, author, committer, options);

        public void Reset(ResetMode resetMode, Commit commit) =>
            repository.Reset(resetMode, commit);

        public void Reset(ResetMode resetMode, Commit commit, CheckoutOptions options) =>
            repository.Reset(resetMode, commit, options);

        public void RemoveUntrackedFiles() =>
            repository.RemoveUntrackedFiles();

        public RevertResult Revert(Commit commit, Signature reverter, RevertOptions options) =>
            repository.Revert(commit, reverter, options);

        public MergeResult Merge(Commit commit, Signature merger, MergeOptions options) =>
            repository.Merge(commit, merger, options);

        public MergeResult Merge(Branch branch, Signature merger, MergeOptions options) =>
            repository.Merge(branch, merger, options);

        public MergeResult Merge(string committish, Signature merger, MergeOptions options) =>
            repository.Merge(committish, merger, options);

        public MergeResult MergeFetchedRefs(Signature merger, MergeOptions options) =>
            repository.MergeFetchedRefs(merger, options);

        public CherryPickResult CherryPick(Commit commit, Signature committer, CherryPickOptions options) =>
            repository.CherryPick(commit, committer, options);

        public BlameHunkCollection Blame(string path, BlameOptions options) =>
            repository.Blame(path, options);

        public FileStatus RetrieveStatus(string filePath) =>
            repository.RetrieveStatus(filePath);

        public RepositoryStatus RetrieveStatus(StatusOptions options) =>
            repository.RetrieveStatus(options);

        public string Describe(Commit commit, DescribeOptions options) =>
            repository.Describe(commit, options);

        public void RevParse(string revision, out Reference reference, out GitObject obj) =>
            repository.RevParse(revision, out reference, out obj);

        public void Dispose() =>
            repository.Dispose();
    }
}