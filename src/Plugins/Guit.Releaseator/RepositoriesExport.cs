using System;
using System.Linq;
using System.Collections.Generic;
using System.Composition;
using LibGit2Sharp;
using System.IO;

namespace Guit.Plugin.Releaseator
{
    [Shared]
    class RepositoriesExport
    {
        readonly Lazy<IEnumerable<IRepository>> repositories;

        [ImportingConstructor]
        public RepositoriesExport(IRepository repository)
        {
            repositories = new Lazy<IEnumerable<IRepository>>(() =>
                 repository.Submodules.Select(x => new Repository(Path.Combine(repository.Info.WorkingDirectory, x.Path))).ToList());
        }

        [Export]
        public IEnumerable<IRepository> Repositories => repositories.Value;
    }
}