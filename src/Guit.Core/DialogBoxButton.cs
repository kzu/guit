using System;
using System.Collections.Generic;
using System.Text;

namespace Guit
{
    [Flags]
    public enum DialogBoxButton
    {
        None = 0,
        Ok = 1,
        Cancel = 2,
        All = 4
    }
}
