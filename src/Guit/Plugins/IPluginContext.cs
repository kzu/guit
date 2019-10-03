using System;
using System.Collections.Generic;
using System.Reflection;

namespace Guit
{
    interface IPluginContext : IDisposable
    {
        IEnumerable<Assembly> GetAssemblies();
    }
}
