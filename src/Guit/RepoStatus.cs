using System;
using System.IO;
using LibGit2Sharp;
using Terminal.Gui;

namespace Guit
{
    class RepoStatus : Label, IRefreshPattern
    {
        Window? window;

        readonly IRepository repository;

        public RepoStatus(IRepository repository)
            : base(string.Empty)
        {
            this.repository = repository;

            Refresh();
        }

        Window? Window => window != null ? window : window = this.GetWindow();

        public void Refresh()
        {
            Text = GetStatus();
        }

        public override void LayoutSubviews()
        {
            if (Window != null)
            {
                Y = -1;
                X = Window.Frame.Width - Text.Length - 3;
            }

            base.LayoutSubviews();
        }

        string GetStatus() =>
            string.Format($"~ {repository.Info.WorkingDirectory.TrimEnd(Path.DirectorySeparatorChar)} @ {repository.Head.FriendlyName} ~");
    }
}