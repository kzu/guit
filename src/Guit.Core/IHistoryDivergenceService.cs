using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace Guit
{
    public interface IHistoryDivergenceService
    {
        IEnumerable<Commit> GetDivergence(IRepository repository, Branch source, Branch target);
        bool HasDivergence(IRepository repository, Branch source, Branch target);
    }
}
