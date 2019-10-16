using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LibGit2Sharp
{
    static class IRepositoryExtensions
    {
        public static string GetName(this IRepository repository) => new DirectoryInfo(repository.Info.WorkingDirectory).Name;
    }
}
