using System.Collections.Generic;
using System.Composition;
using System.Security;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit
{
    [Shared]
    [ContentView("Plugins", '6')]
    public class PluginsView : ContentView
    {
        readonly IRepository repository;

        ListView view;

        [ImportingConstructor]
        public PluginsView(IRepository repository)
            : base("Plugins")
        {
            this.repository = repository;

            view = new ListView(new List<string> { "Super", "Duper" })
            {
                AllowsMarking = true
            };

            Content = view;
        }
    }
}
