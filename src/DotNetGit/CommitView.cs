using System.Linq;
using LibGit2Sharp;
using Terminal.Gui;

namespace DotNetGit
{
    public class CommitView : FrameView
    {
        public CommitView() : base(new Rect(0, 0, 100, 100), "Commit")
        {
            var status = App.Repository.RetrieveStatus(new StatusOptions());
            var list = status
                .Added.Concat(status.Untracked).Select(x => new FileStatus { Path = x.FilePath, Status = Status.Added })
                .Concat(status.Removed.Concat(status.Missing).Select(x => new FileStatus { Path = x.FilePath, Status = Status.Deleted }))
                .Concat(status.Modified.Select(x => new FileStatus { Path = x.FilePath, Status = Status.Modified }))
                .OrderBy(x => x.Path)
                .ToList();

            Add(new ListView(new Rect(5, 5, 95, 95), list));
        }

        class FileStatus
        {
            public string Path { get; set; }
            public Status Status { get; set; }

            public override string ToString()
            {
                switch (Status)
                {
                    case Status.Added:
                        return "+ " + Path;
                    case Status.Deleted:
                        return "x " + Path;
                    case Status.Modified:
                        return "* " + Path;
                    default:
                        return "+ " + Path;
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
