using System;
using System.Threading;
using System.Threading.Tasks;

namespace Guit
{
    class MenuCommandAdapter : IMenuCommand
    {
        readonly Lazy<ContentView, MenuCommandMetadata> view;
        readonly IApp app;

        public MenuCommandAdapter(Lazy<ContentView, MenuCommandMetadata> view, IApp app)
        {
            this.view = view;
            this.app = app;
        }

        public async Task ExecuteAsync(CancellationToken cancellation) =>
            await app.RunAsync(view.Value);
    }
}