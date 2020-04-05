using System;
using System.Collections.Generic;
using System.Text;
using LibGit2Sharp;

namespace Guit.Plugin.Changes
{
    public interface IChangesView : IRefreshPattern
    {
        IEnumerable<StatusEntry> GetMarkedEntries(bool? submoduleEntriesOnly = default);
    }
}