using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using Merq;
using Guit.Events;
using System.IO;

namespace LibGit2Sharp
{
    /// <summary>
    /// Usability overloads for <see cref="IRepository"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class IRepositoryExtensions
    {
        /// <summary>
        /// Reverts the given <paramref name="filePaths"/> to the current head state.
        /// </summary>
        public static void RevertFileChanges(this IRepository repository, params string[] filePaths) =>
            repository.CheckoutPaths(
                repository.Head.FriendlyName,
                filePaths,
                new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });

        public static IEnumerable<string> GetBranchNames(this IRepository repository) =>
            repository
                .Branches
                .Select(x => x.GetName())
                .Distinct()
                .OrderBy(x => x);

        public static IEnumerable<string> GetRemoteNames(this IRepository repository) =>
            repository
                .Network
                .Remotes
                .Select(x => x.Name)
                .Distinct()
                .OrderBy(x => x);

        public static string GetDefaultRemoteName(this IRepository repository, string defaultRemoteName = "origin") =>
            repository.GetRemoteNames().Contains(defaultRemoteName) ? defaultRemoteName : repository.GetRemoteNames().FirstOrDefault();

        public static void UpdateSubmodules(this IRepository repository, bool recursive = true, IEventStream? eventStream = null)
        {
            foreach (var submodule in repository.Submodules)
            {
                eventStream?.Push(Status.Create("Submodule update {0}", submodule.Name));

                repository.Submodules.Update(submodule.Name, new SubmoduleUpdateOptions());

                if (recursive)
                {
                    using (var subRepository = new Repository(Path.Combine(repository.Info.WorkingDirectory, submodule.Path)))
                        subRepository.UpdateSubmodules(eventStream: eventStream);
                }
            }
        }
    }
}