using System;
using System.Collections.Generic;
using System.Text;

namespace Guit
{
    class NullProgressStatus : IDisposable
    {
        public void Dispose() { }
    }
}