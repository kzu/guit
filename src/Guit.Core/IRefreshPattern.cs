using System;
using System.Collections.Generic;
using System.Text;

namespace Guit
{
    /// <summary>
    /// Represents an item that supports content refresh
    /// </summary>
    public interface IRefreshPattern
    {
        /// <summary>
        /// Updates the content of the item
        /// </summary>
        void Refresh();
    }
}
