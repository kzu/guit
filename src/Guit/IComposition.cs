using System;
using System.Collections.Generic;

namespace Guit
{
    interface IComposition : IDisposable
    {
        T GetExport<T>();

        IEnumerable<T> GetExports<T>();
    }
}
