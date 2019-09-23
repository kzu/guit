using System;

namespace LibGit2Sharp
{
    static class IRepositoryExtensions
    {
        public static void RevertFileChanges(this IRepository repository, params string[] filePaths) =>
            repository.CheckoutPaths(
                repository.Head.FriendlyName,
                filePaths,
                new CheckoutOptions { CheckoutModifiers = CheckoutModifiers.Force });
    }
}