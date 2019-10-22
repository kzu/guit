using System;
using System.Collections.Generic;
using System.Text;

namespace Guit
{
    public interface IDynamicMenuCommand : IMenuCommand
    {
        bool IsVisible { get; }

        bool IsEnabled { get; }
    }
}
