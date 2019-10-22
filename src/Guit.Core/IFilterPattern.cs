using System;
using System.Collections.Generic;
using System.Text;

namespace Guit
{
    /// <summary>
    /// Represents an item that supports content filtering
    /// </summary>
    public interface IFilterPattern
    {
        /// <summary>
        /// Gets or set the filters
        /// </summary>
        string[]? Filter { get; set; }
    }
}
