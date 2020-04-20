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
    /// <summary>
    /// 
    /// </summary>
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

        /// <summary>
        /// Gets the entire history of the branch
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public string GetFullPath(string filePath) =>
            Path.IsPathRooted(filePath) ? Path.GetFullPath(filePath) :
            Path.GetFullPath(Path.Combine(repository.Info.WorkingDirectory, filePath));

        /// <summary>
        /// reverts a file to the previous chang
        /// ?Can you choose the revert. is it changed back to the last commit?
        /// </summary>
        /// <param name="filePaths"></param>
        public void RevertFileChanges(params string[] filePaths) =>
            repository.CheckoutPaths(
                repository.Head.FriendlyName,
                filePaths,
                new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });

        /// <summary>
        /// Gets the names of all branches
        /// </summary>
        /// <returns>
        /// IEnumerable(? Still not sure what exaclty ab IEnumerable is) of branch names
        /// </returns>
        public IEnumerable<string> GetBranchNames() =>
            repository
                .Branches
                .Select(x => x.GetName())
                .Distinct()
                .OrderBy(x => x);

        /// <summary>
        /// Gets the names of all remotes (all nonlocal repositories)
        /// </summary>
        /// <returns>
        /// IEnumerable of remote names
        /// </returns>
        public IEnumerable<string> GetRemoteNames() =>
            repository
                .Network
                .Remotes //repositories that aren't local
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x);

      
        /// <summary>
        /// Gets the default remote name.
        /// paramater defaults to "origin"
        /// </summary>
        /// <param name="defaultRemoteName"></param>
        /// <returns>
        /// returns string of default remote name
        /// </returns>
        public string GetDefaultRemoteName(string defaultRemoteName = "origin") =>
            GetRemoteNames().Contains(defaultRemoteName) ? defaultRemoteName : GetRemoteNames().FirstOrDefault();

        //
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

        //A lotta different options for fetch

        /// <summary>
        /// Fetches. There's a lot of different fetch methods and I'm not sure what they do differently
        /// </summary>
        /// <param name="credentials"></param>
        /// <param name="eventStream"></param>
        /// <param name="prune"></param>
        public void Fetch(CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false) =>
            Fetch(repository.Network.Remotes, credentials, eventStream, prune);

        /// <summary>
        /// Fetches
        /// </summary>
        /// <param name="remoteName"></param>
        /// <param name="credentials"></param>
        /// <param name="eventStream"></param>
        /// <param name="prune"></param>
        public void Fetch(string remoteName, CredentialsHandler credentials, IEventStream? eventStream = null, bool prune = false)
        {
            if (repository.Network.Remotes.FirstOrDefault(x => x.Name == remoteName) is Remote remote)
                Fetch(remote, credentials, eventStream, prune);
        }

        /// <summary>
        /// Fetches
        /// </summary>
        /// <param name="remote"></param>
        /// <param name="credentials"></param>
        /// <param name="eventStream"></param>
        /// <param name="prune"></param>
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
        /// <summary>
        /// Makes a new branch
        /// </summary>
        /// <param name="branchName">branch name</param>
        /// <returns>
        /// Not sure what exactly it returns, whether its the location of the new branch or what.
        /// </returns>
        public Branch CreateBranch(string branchName) =>
            RepositoryExtensions.CreateBranch(repository, branchName);
        /// <summary>
        /// Checks out the selected branch
        /// </summary>
        /// <param name="branch"></param>
        public void Checkout(Branch branch) =>
            Git.Checkout(repository, branch);
        /// <summary>
        /// Stages the filepath selected
        /// </summary>
        /// <param name="filepath"></param>
        public void Stage(string filepath) =>
            Git.Stage(repository, filepath);

        /// <summary>
        /// Removes a file from the repository
        /// </summary>
        /// <param name="filepath"></param>
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
        /// <summary>
        /// Checks out a whole tree
        /// What's a tree? Is is a bigger bunch of the branch? Is it the entire library of all the code ever?
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="paths"></param>
        /// <param name="opts"></param>
        public void Checkout(Tree tree, IEnumerable<string> paths, CheckoutOptions opts) =>
            repository.Checkout(tree, paths, opts);
        /// <summary>
        /// Checks out a path
        /// Not sure the difference between the checkouts. 
        /// </summary>
        /// <param name="committishOrBranchSpec"></param>
        /// <param name="paths"></param>
        /// <param name="checkoutOptions"></param>
        public void CheckoutPaths(string committishOrBranchSpec, IEnumerable<string> paths, CheckoutOptions checkoutOptions) =>
            repository.CheckoutPaths(committishOrBranchSpec, paths, checkoutOptions);
        /// <summary>
        /// Looks up an object based on the object's ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>
        /// not sure what specific information it returns. Or is it the object itself?
        /// </returns>
        public GitObject Lookup(ObjectId id) =>
            repository.Lookup(id);

        /// <summary>
        /// try to lookup an object by its sha or a reference canonical name. If no matching
        /// object is found, null will be returned.
        /// What is "objectish"? a string representation of the object?
        /// </summary>
        /// <param name="objectish">A revparse spec for the object to lookup.</param>
        /// <returns>The LibGit2Sharp.GitObject or null if it was not found.</returns>
        public GitObject Lookup(string objectish) =>
            repository.Lookup(objectish);

        /// <summary>
        /// Try to lookup an object by its LibGit2Sharp.ObjectId and LibGit2Sharp.ObjectType.
        /// If no matching object is found, null will be returned.
        /// </summary>
        /// <param name="id">The id to lookup.</param>
        /// <param name="type">The kind of GitObject being looked up</param>
        /// <returns> The LibGit2Sharp.GitObject or null if it was not found.</returns>
        public GitObject Lookup(ObjectId id, ObjectType type) =>
            repository.Lookup(id, type);

        /// <summary>
        /// Try to lookup an object by its sha or a reference canonical name and LibGit2Sharp.ObjectType.
        /// If no matching object is found, null will be returned.
        /// </summary>
        /// <param name="objectish">A revparse spec for the object to lookup.</param>
        /// <param name="type">The kind of LibGit2Sharp.GitObject being looked up</param>
        /// <returns>The LibGit2Sharp.GitObject or null if it was not found.</returns>
        public GitObject Lookup(string objectish, ObjectType type) =>
            repository.Lookup(objectish, type);

        /// <summary>
        /// Makes a commit
        /// Stores the content of the LibGit2Sharp.Repository.Index as a new LibGit2Sharp.Commit
        /// into the repository. The tip of the LibGit2Sharp.Repository.Head will be used 
        /// as the parent of this new Commit. Once the commit is created, the LibGit2Sharp.Repository.Head
        /// will move forward to point at it.
        /// </summary>
        /// <param name="message">The description of why a change was made to the repository.</param>
        /// <param name="author">  The LibGit2Sharp.Signature of who made the change.</param>
        /// <param name="committer">The LibGit2Sharp.Signature of who added the change to the repository.</param>
        /// <param name="options">The LibGit2Sharp.CommitOptions that specify the commit behavior.</param>
        /// <returns>The generated LibGit2Sharp.Commit.</returns>
        public Commit Commit(string message, Signature author, Signature committer, CommitOptions options) =>
            repository.Commit(message, author, committer, options);
        /// <summary>
        /// resets... something. Not sure what specifically. Is is one singular commit, or a larger thing,
        /// </summary>
        /// <param name="resetMode"></param>
        /// <param name="commit"></param>
        
        public void Reset(ResetMode resetMode, Commit commit) =>
            repository.Reset(resetMode, commit);
        /// <summary>
        ///  Sets LibGit2Sharp.IRepository.Head to the specified commit and optionally resets
        ///  the LibGit2Sharp.IRepository.Index and the content of the working tree to match.
        /// </summary>
        /// <param name="resetMode">Flavor of reset operation to perform.</param>
        /// <param name="commit">The target commit object.</param>
        /// <param name="options">Collection of parameters controlling checkout behavior.</param>
        public void Reset(ResetMode resetMode, Commit commit, CheckoutOptions options) =>
            repository.Reset(resetMode, commit, options);
        
        /// <summary>
        /// Clean the working tree by removing files that are not under version control.
        /// What files would be untracked? is this just a cleanup when you're fixing up your history or something?
        /// </summary>
        public void RemoveUntrackedFiles() =>
            repository.RemoveUntrackedFiles();

        /// <summary>
        /// Revert the specified commit. Whats the difference between this and RevertFileChanges?
        /// </summary>
        /// <param name="commit">The LibGit2Sharp.IRepository.Commit(System.String,LibGit2Sharp.Signature,LibGit2Sharp.Signature,LibGit2Sharp.CommitOptions)
        /// to revert.</param>
        /// <param name="reverter">The LibGit2Sharp.Signature of who is performing the reverte.</param>
        /// <param name="options">LibGit2Sharp.RevertOptions controlling revert behavior.</param>
        /// <returns>The result of the revert.</returns>
        public RevertResult Revert(Commit commit, Signature reverter, RevertOptions options) =>
            repository.Revert(commit, reverter, options);

        /// <summary>
        /// Merge changes from commit into the branch pointed at by HEAD..
        /// </summary>
        /// <param name="commit">The commit to merge into the branch pointed at by HEAD.</param>
        /// <param name="merger">The LibGit2Sharp.Signature of who is performing the merge.</param>
        /// <param name="options">Specifies optional parameters controlling merge behavior; 
        /// if null, the defaultsare used.</param>
        /// <returns>The LibGit2Sharp.MergeResult of the merge.</returns>
        public MergeResult Merge(Commit commit, Signature merger, MergeOptions options) =>
            repository.Merge(commit, merger, options);

        /// <summary>
        /// Merges changes from branch into the branch pointed at by HEAD.
        /// </summary>
        /// <param name="branch">The branch to merge into the branch pointed at by HEAD.</param>
        /// <param name="merger">The LibGit2Sharp.Signature of who is performing the merge.</param>
        /// <param name="options">Specifies optional parameters controlling merge behavior; 
        /// if null, the defaults are used.</param>
        /// <returns> The LibGit2Sharp.MergeResult of the merge.</returns>
        public MergeResult Merge(Branch branch, Signature merger, MergeOptions options) =>
            repository.Merge(branch, merger, options);
        /// <summary>
        /// Merge, based on a string version of the commit. 
        /// </summary>
        /// <param name="committish"></param>
        /// <param name="merger"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public MergeResult Merge(string committish, Signature merger, MergeOptions options) =>
            repository.Merge(committish, merger, options);

        /// <summary>
        /// Merge the reference that was recently fetched. This will merge the branch on
        /// the fetched remote that corresponded to the current local branch when we did
        /// the fetch. This is the second step in performing a pull operation (after having
        /// performed said fetch).
        /// </summary>
        /// <param name="merger">The LibGit2Sharp.Signature of who is performing the merge.</param>
        /// <param name="options">Specifies optional parameters controlling merge behavior;
        /// if null, the defaults are used.</param>
        /// <returns>The LibGit2Sharp.MergeResult of the merge.</returns>
        public MergeResult MergeFetchedRefs(Signature merger, MergeOptions options) =>
            repository.MergeFetchedRefs(merger, options);

        /// <summary>
        /// Cherry picks changes from the commit into the branch pointed at by HEAD. 
        /// Not sure what that means.
        /// </summary>
        /// <param name="commit">The commit to cherry pick into branch pointed at by HEAD.</param>
        /// <param name="committer">The LibGit2Sharp.Signature of who is performing the cherry pick.</param>
        /// <param name="options"> Specifies optional parameters controlling cherry pick behavior; if null, the defaults are used</param>
        /// <returns>The LibGit2Sharp.MergeResult of the merge.</returns>
        public CherryPickResult CherryPick(Commit commit, Signature committer, CherryPickOptions options) =>
            repository.CherryPick(commit, committer, options);

        /// <summary>
        ///  Find where each line of a file originated. 
        /// </summary>
        /// <param name="path">Path of the file to blame.</param>
        /// <param name="options"> Specifies optional parameters; if null, the defaults are used.</param>
        /// <returns>The blame for the file.</returns>
        public BlameHunkCollection Blame(string path, BlameOptions options) =>
            repository.Blame(path, options);

        /// <summary>
        ///  Retrieves the state of a file in the working directory, comparing it against
        ///  the staging area and the latest commit.
        /// </summary>
        /// <param name="filePath">The relative path within the working directory to the file.</param>
        /// <returns> A LibGit2Sharp.FileStatus representing the state of the filePath parameter.</returns>
        public FileStatus RetrieveStatus(string filePath) =>
            repository.RetrieveStatus(filePath);

        /// <summary>
        ///Retrieves the state of all files in the working directory, comparing them against
        ///the staging area and the latest commit.
        /// </summary>
        /// <param name="options">If set, the options that control the status investigation.</param>
        /// <returns>A LibGit2Sharp.RepositoryStatus holding the state of all the files.</returns>
        public RepositoryStatus RetrieveStatus(StatusOptions options) =>
            repository.RetrieveStatus(options);
        /// <summary>
        ///Finds the most recent annotated tag that is reachable from a commit.
        ///If the tag points to the commit, then only the tag is shown. Otherwise, it suffixes
        ///the tag name with the number of additional commits on top of the tagged object
        ///and the abbreviated object name of the most recent commit.
        ///Optionally, the options parameter allow to tweak the search strategy (considering
        ///lightweith tags, or even branches as reference points) and the formatting of
        ///the returned identifier.
        /// </summary>
        /// <param name="commit">The commit to be described.</param>
        /// <param name="options">Determines how the commit will be described.</param>
        /// <returns>A descriptive identifier for the commit based on the nearest annotated tag.</returns>
        public string Describe(Commit commit, DescribeOptions options) =>
            repository.Describe(commit, options);

        /// <summary>
        /// Parses out a revision?
        /// "Parse an extended SHA-1 expression and retrieve the object and the reference mentioned in the revision (if any)"
        /// Not sure what a SHA-1 is.
        /// </summary>
        /// <param name="revision"></param>
        /// <param name="reference"></param>
        /// <param name="obj"></param>
        public void RevParse(string revision, out Reference reference, out GitObject obj) =>
            repository.RevParse(revision, out reference, out obj);
        
        /// <summary>
        /// "Performs application-defined tasked associated with freeing, releasing, or resetting unmanaged resources"
        /// So, when there is an unmanaged resource found, Dispose() gits rid of it in some way.
        /// </summary>
        public void Dispose() =>
            repository.Dispose();
    }
}