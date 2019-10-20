using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Composition;
using LibGit2Sharp;

namespace Guit
{
    [Shared]
    [Export(typeof(IHistoryDivergenceService))]
    class HistoryDivergenceService : IHistoryDivergenceService
    {
        const int MaxCachedItems = 10;

        readonly ConcurrentDictionary<string, (DateTime LastAccessTime, Dictionary<string, Commit> CommitsBySha)> cache =
            new ConcurrentDictionary<string, (DateTime LastAccessTime, Dictionary<string, Commit> CommitsBySha)>();

        public IEnumerable<Commit> GetDivergence(IRepository repository, Branch source, Branch target)
        {
            try
            {
                if (HasDivergence(repository, source, target, out var aheadBy))
                {
                    var sourceCommits = GetCachedCommitsBySha(source);
                    var targetCommits = GetCachedCommitsBySha(target);

                    return sourceCommits
                        .Where(x => !targetCommits.ContainsKey(x.Value.Sha))
                        .Select(x => x.Value)
                        .Take(aheadBy);
                }

                return Enumerable.Empty<Commit>();
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

            if (historyDivergence.AheadBy.HasValue && historyDivergence.AheadBy.Value > 0)
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