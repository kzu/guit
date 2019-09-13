using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using Guit.Events;
using LibGit2Sharp;
using Merq;
using Terminal.Gui;

namespace Guit.Plugin.Changes
{
    [Shared]
    [Export]
    [Export(typeof(ContentView))]
    public class ChangesView : ContentView
    {
        readonly IEventStream eventStream;

        readonly List<FileStatus> files;
        readonly ListView view;

        [ImportingConstructor]
        public ChangesView(Repository repository, IEventStream eventStream)
            : base("Changes")
        {
            this.eventStream = eventStream;

            var status = repository.RetrieveStatus(new StatusOptions());
            files = status
                .Added.Concat(status.Untracked).Select(x => new FileStatus { Entry = x, Status = Status.Added })
                .Concat(status.Removed.Concat(status.Missing).Select(x => new FileStatus { Entry = x, Status = Status.Deleted }))
                .Concat(status.Modified.Select(x => new FileStatus { Entry = x, Status = Status.Modified }))
                .OrderBy(x => x.Entry.FilePath)
                .ToList();

            view = new ListView(files)
            {
                AllowsMarking = true,
            };

            // Mark modified files by default
            foreach (var file in files.Where(x => x.Status == Status.Modified))
                view.Source.SetMark(files.IndexOf(file), true);

            view.SelectedChanged += OnSelectedChanged;

            Content = view;
        }

        public override string Context => nameof(Changes);

        void OnSelectedChanged()
        {
            eventStream.Push<SelectionChanged>(files[view.SelectedItem].Entry);
        }

        public IEnumerable<StatusEntry> GetMarkedEntries() =>
            files.Where(x => view.Source.IsMarked(files.IndexOf(x))).Select(x => x.Entry);

        class FileStatus
        {
            public StatusEntry Entry { get; set; }

            public Status Status { get; set; }

            public override string ToString()
            {
                switch (Status)
                {
                    case Status.Added:
                        return "+ " + Entry.FilePath;
                    case Status.Deleted:
                        return "- " + Entry.FilePath;
                    case Status.Modified:
                        return "* " + Entry.FilePath;
                    default:
                        return "+ " + Entry.FilePath;
                }
            }
        }

        enum Status
        {
            Added,
            Deleted,
            Modified,
        }
    }
}