using System;
using System.Collections.Generic;
using System.Text;

namespace Guit
{
    public class NullColumnDefinition<T> : ColumnDefinition<T>
    {
        public NullColumnDefinition() : base(x => string.Empty, 0)
        { }
    }
}
