using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace Guit
{
    public interface IHistoryDivergenceService
    {
        bool TryGetDivergence(IRepository repository, Branch source, Branch target, out IEnumerable<Commit> commits, bool filterSimilarCommits = false);

        bool HasDivergence(IRepository repository, Branch source, Branch target);
    }
}
