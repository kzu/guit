using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using LibGit2Sharp;
using System.Globalization;

namespace Guit
{
    [Shared]
    [Export(typeof(IHistoryDivergenceService))]
    class HistoryDivergenceService : IHistoryDivergenceService
    {
        const int MaxCachedItems = 20;

        readonly ConcurrentDictionary<string, (DateTime LastAccessTime, Dictionary<string, Commit> CommitsBySha)> cache =
            new ConcurrentDictionary<string, (DateTime LastAccessTime, Dictionary<string, Commit> CommitsBySha)>();

        public bool TryGetDivergence(IRepository repository, Branch source, Branch target, out IEnumerable<Commit> commits, bool filterSimilarCommits = false)
        {
            try
            {
                if (HasDivergence(repository, source, target, out var aheadBy))
                {
                    var sourceCommits = GetCachedCommitsBySha(source);
                    var targetCommits = GetCachedCommitsBySha(target);

                    commits = sourceCommits
                        .Where(x => !targetCommits.ContainsKey(x.Value.Sha))
                        .Where(x => !filterSimilarCommits ||
                                    !targetCommits.Any(targetCommit =>
                                        string.Compare(x.Value.Message, targetCommit.Value.Message, CultureInfo.CurrentCulture, System.Globalization.CompareOptions.IgnoreSymbols) == 0))
                        .Select(x => x.Value)
                        .Take(aheadBy);

                    return true;
                }

                commits = Enumerable.Empty<Commit>();

                return false;
            }
            finally
            {
                PruneCache();
            }
        }

        public bool HasDivergence(IRepository repository, Branch source, Branch target) =>
            HasDivergence(repository, source, target, out var _);

        bool HasDivergence(IRepository repository, Branch source, Branch target, out int aheadBy)
        {
            aheadBy = 0;

            var historyDivergence = repository.ObjectDatabase.CalculateHistoryDivergence(source.Tip, target.Tip);

            if (historyDivergence.AheadBy.HasValue)
                aheadBy = historyDivergence.AheadBy.Value;

            return aheadBy > 0;
        }

        Dictionary<string, Commit> GetCachedCommitsBySha(Branch branch)
        {
            var key = $"{branch.CanonicalName}@{branch.Tip.GetShortSha()}";

            var cacheEntry = cache.GetOrAdd(key, x => (DateTime.Now, branch.Commits.ToDictionary(commit => commit.Sha)));
            cacheEntry.LastAccessTime = DateTime.Now;

            return cacheEntry.CommitsBySha;
        }

        void PruneCache()
        {
            if (cache.Count > MaxCachedItems)
            {
                foreach (var cacheEntry in cache.OrderBy(x => x.Value.LastAccessTime))
                    if (cache.TryRemove(cacheEntry.Key, out var _) && cache.Count <= MaxCachedItems)
                        return;
            }
        }
    }
}