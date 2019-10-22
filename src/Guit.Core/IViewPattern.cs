using System;
using System.Collections.Generic;
using System.Text;

namespace Guit
{
    /// <summary>
    /// Represents an item that be opened with a viewer
    /// </summary>
    public interface IViewPattern
    {
        /// <summary>
        /// Opens an item by its corresponding viewer
        /// </summary>
        void View();
    }
}
