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

        /// <inheritdoc />
        public string GetFullPath(string filePath) =>
            Path.IsPathRooted(filePath) ? Path.GetFullPath(filePath) :
            Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, filePath));

        //// <inheritdoc />
        public void RevertFileChanges(params string[] filePaths) =>
            repository.CheckoutPaths(
                repository.Head.FriendlyName,
                filePaths,
                new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });

        /// <inheritdoc />
        public IEnumerable<string> GetBranchNames() =>
            repository
                .Branches
                .Select(x => x.GetName())
                .Distinct()
                .OrderBy(x => x);

        /// <inheritdoc />
        public IEnumerable<string> GetRemoteNames() =>
            repository
                .Network
                .Remotes //repositories that aren't local
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x);

        /// <inheritdoc />
        public string GetDefaultRemoteName(string defaultRemoteName = "origin") =>
            GetRemoteNames().Contains(defaultRemoteName) ? defaultRemoteName : GetRemoteNames().FirstOrDefault();

        /// <inheritdoc />
        /// <summary>
        /// Not quite sure, but updates the submodules. Goes through everything(?) and pushes any chages?
        /// </summary>
        /// <param name="recursive"></param>
        /// <param name="eventStream"></param>
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

        /// <inheritdoc />
        public void Fetch(CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false) =>
            Fetch(repository.Network.Remotes, credentials, eventStream, prune);

        /// <inheritdoc />
        public void Fetch(string remoteName, CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false)
        {
            if (repository.Network.Remotes.FirstOrDefault(x => x.Name == remoteName) is Remote remote)
                Fetch(remote, credentials, eventStream, prune);
        }

        /// <inheritdoc />
        public void Fetch(Remote remote, CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false) =>
            Fetch(new Remote[] { remote }, credentials, eventStream, prune);
        
        /// <summary>
        /// The fetch all the other fetches are based on.
        /// </summary>
        /// <param name="remotes"></param>
        /// <param name="credentials"></param>
        /// <param name="eventStream"></param>
        /// <param name="prune"></param>
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

        /// <inheritdoc />
        public Branch CreateBranch(string branchName) =>
            RepositoryExtensions.CreateBranch(repository, branchName);

        /// <inheritdoc />
        public void Checkout(Branch branch) =>
            Git.Checkout(repository, branch);
        
        /// <inheritdoc />
        public void Stage(string filepath) =>
            Git.Stage(repository, filepath);

        /// <inheritdoc />
        public void Remove(string filepath) =>
            Git.Remove(repository, filepath);
       
        /// <summary>
        /// gets a list of commits that havent been rebased to the branch.
        /// I'm not sure exactl what that means.
        /// </summary>
        /// <param name="branch"></param>
        /// <returns>
        /// IEnumerable of commits that haven't been rebased.
        /// </returns>
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
        /// <summary>
        /// Gets the URL of the repository.
        /// Not sure it it's the local rep or the repo online, or what.
        /// </summary>
        /// <returns>
        /// String of repository URL
        /// </returns>
        public string GetRepoUrl()
        {
            var repoUrl = repository.Config.GetValueOrDefault<string>("remote.origin.url");
            if (repoUrl.EndsWith(GitSuffix))
                repoUrl = repoUrl.Remove(repoUrl.Length - GitSuffix.Length);

            return repoUrl;
        }

        /// <summary>
        /// Not sure. Does it just open a url of where that commit is? 
        /// </summary>
        /// <param name="commit"></param>
        public void OpenUrl(Commit commit) =>
            Process.Start("cmd", $"/c start {GetRepoUrl()}/commit/{commit.Sha}");

        /// <inheritdoc />
        public void Checkout(Tree tree, IEnumerable<string> paths, CheckoutOptions opts) =>
            repository.Checkout(tree, paths, opts);

        /// <inheritdoc />
        public void CheckoutPaths(string committishOrBranchSpec, IEnumerable<string> paths, CheckoutOptions checkoutOptions) =>
            repository.CheckoutPaths(committishOrBranchSpec, paths, checkoutOptions);
        
        /// <inheritdoc />
        public GitObject Lookup(ObjectId id) =>
            repository.Lookup(id);

        /// <inheritdoc />
        public GitObject Lookup(string objectish) =>
            repository.Lookup(objectish);

        /// <inheritdoc />
        public GitObject Lookup(ObjectId id, ObjectType type) =>
            repository.Lookup(id, type);

        /// <inheritdoc />
        public GitObject Lookup(string objectish, ObjectType type) =>
            repository.Lookup(objectish, type);

        /// <inheritdoc />
        public Commit Commit(string message, Signature author, Signature committer, CommitOptions options) =>
            repository.Commit(message, author, committer, options);
        
        /// <inheritdoc />
        public void Reset(ResetMode resetMode, Commit commit) =>
            repository.Reset(resetMode, commit);

        /// <inheritdoc />
        public void Reset(ResetMode resetMode, Commit commit, CheckoutOptions options) =>
            repository.Reset(resetMode, commit, options);

        /// <inheritdoc />
        public void RemoveUntrackedFiles() =>
            repository.RemoveUntrackedFiles();

        /// <inheritdoc />
        public RevertResult Revert(Commit commit, Signature reverter, RevertOptions options) =>
            repository.Revert(commit, reverter, options);

        /// <inheritdoc />
        public MergeResult Merge(Commit commit, Signature merger, MergeOptions options) =>
            repository.Merge(commit, merger, options);

        /// <inheritdoc />
        public MergeResult Merge(Branch branch, Signature merger, MergeOptions options) =>
            repository.Merge(branch, merger, options);
        
        /// <inheritdoc />
        public MergeResult Merge(string committish, Signature merger, MergeOptions options) =>
            repository.Merge(committish, merger, options);

        /// <inheritdoc />
        public MergeResult MergeFetchedRefs(Signature merger, MergeOptions options) =>
            repository.MergeFetchedRefs(merger, options);

        /// <inheritdoc />
        public CherryPickResult CherryPick(Commit commit, Signature committer, CherryPickOptions options) =>
            repository.CherryPick(commit, committer, options);

        /// <inheritdoc />
        public BlameHunkCollection Blame(string path, BlameOptions options) =>
            repository.Blame(path, options);

        /// <inheritdoc />
        public FileStatus RetrieveStatus(string filePath) =>
            repository.RetrieveStatus(filePath);

        /// <inheritdoc />
        public RepositoryStatus RetrieveStatus(StatusOptions options) =>
            repository.RetrieveStatus(options);
        /// <inheritdoc />
        public string Describe(Commit commit, DescribeOptions options) =>
            repository.Describe(commit, options);

        /// <summary>
        /// Parses out a revision?
        /// <inheritdoc />
        /// Not sure what a SHA-1 is.
        /// </summary>
        /// <param name="revision"></param>
        /// <param name="reference"></param>
        /// <param name="obj"></param>
        public void RevParse(string revision, out Reference reference, out GitObject obj) =>
            repository.RevParse(revision, out reference, out obj);

        /// <inheritdoc />
        /// <summary>
        /// So, when there is an unmanaged resource found, Dispose() gits rid of it in some way.
        /// </summary>
        public void Dispose() =>
            repository.Dispose();
    }
}