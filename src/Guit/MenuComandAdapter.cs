using System;
using System.Threading;
using System.Threading.Tasks;

namespace Guit
{
    class MenuCommandAdapter : IMenuCommand
    {
        readonly Lazy<ContentView, MenuCommandMetadata> view;
        readonly IShell app;

        public MenuCommandAdapter(Lazy<ContentView, MenuCommandMetadata> view, IShell app)
        {
            this.view = view;
            this.app = app;
        }

        public async Task ExecuteAsync(CancellationToken cancellation = default) => 
            await app.RunAsync(view.Value);
    }
}