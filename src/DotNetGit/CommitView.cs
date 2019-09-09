using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using DotNetGit.Events;
using LibGit2Sharp;
using Merq;
using Terminal.Gui;

namespace DotNetGit
{
    [Shared]
    [Export]
    public class CommitView : FrameView
    {
        readonly IEventStream eventStream;
        readonly List<FileStatus> files;
        readonly ListView view;

        [ImportingConstructor]
        public CommitView(Repository repository, IEventStream eventStream) : base("Changes")
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
            view.SelectedChanged += OnSelectedChanged;
            Add(view);

            Width = Dim.Fill();
            Height = Dim.Fill();
        }

        void OnSelectedChanged()
        {
            eventStream.Push<SelectionChanged>(files[view.SelectedItem].Entry);
        }

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
