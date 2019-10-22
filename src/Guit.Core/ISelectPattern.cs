using System;
using System.Collections.Generic;
using System.Text;

namespace Guit
{
    /// <summary>
    /// Represents an item that supports content selection
    /// </summary>
    public interface ISelectPattern
    {
        /// <summary>
        /// Select/Unselect all items
        /// </summary>
        /// <param name="invertSelection">If true, invert the selection when all items have the same selection value</param>
        void SelectAll(bool invertSelection = true);
    }
}