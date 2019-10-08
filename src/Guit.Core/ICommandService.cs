using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Guit
{
    public interface ICommandService
    {
        Task RunAsync(string commandId, CancellationToken cancellation = default);
    }
}
