using System.Linq;
using System.Composition;
using LibGit2Sharp;
using Terminal.Gui;
using System.Collections.Generic;

namespace Guit.Plugin.Sync
{
    [Shared]
    [Export]
    [ContentView(nameof(Sync), '2')]
    public class SyncView : ContentView
    {
        readonly IRepository repository;
        readonly IHistoryDivergenceService historyDivergenceService;

        readonly ListView<Commit> aheadListView;
        readonly ListView<Commit> behindListView;
        readonly FrameView aheadFrameView;
        readonly FrameView behindFrameView;

        readonly ColumnDefinition<Commit>[] columnDefinitions = new[]
        {
                new ColumnDefinition<Commit>(x => x.GetShortSha(), 10),
                new ColumnDefinition<Commit>(x => x.MessageShort, "*")
        };

        [ImportingConstructor]
        public SyncView(IRepository repository, IHistoryDivergenceService historyDivergenceService)
            : base("Sync")
        {
            this.repository = repository;
            this.historyDivergenceService = historyDivergenceService;

            aheadListView = new ListView<Commit>(columnDefinitions);
            aheadFrameView = new FrameView(string.Empty) { Y = 1, Width = Dim.Percent(50) };
            aheadFrameView.Add(aheadListView);

            behindListView = new ListView<Commit>(columnDefinitions);
            behindFrameView = new FrameView(string.Empty) { Y = 1, Width = Dim.Percent(100) };
            behindFrameView.Add(behindListView);

            Content = new StackPanel(StackPanelOrientation.Horizontal, aheadFrameView, behindFrameView);
        }

        public override void Refresh()
        {
            base.Refresh();

            var sourceBranch = repository.Head;
            var targetBranch = GetCandidateTargetBranches(sourceBranch)
                .FirstOrDefault(target => HasDivergence(sourceBranch, target));

            if (targetBranch != null)
            {
                aheadListView.SetValues(historyDivergenceService.GetDivergence(repository, sourceBranch, targetBranch).ToList());
                behindListView.SetValues(historyDivergenceService.GetDivergence(repository, targetBranch, sourceBranch).ToList());

                aheadFrameView.Title = string.Format("{0} commits ahead {1}", aheadListView.Values.Count(), targetBranch.FriendlyName);
                behindFrameView.Title = string.Format("{0} commits behind {1}", behindListView.Values.Count(), targetBranch.FriendlyName);
            }
            else
            {
                aheadFrameView.Title = "Up to date!";
                behindFrameView.Title = "Up to date!";
            }
        }

        bool HasDivergence(Branch source, Branch? target) =>
            target != null &&
            (historyDivergenceService.HasDivergence(repository, source, target) ||
             historyDivergenceService.HasDivergence(repository, target, source));

        IEnumerable<Branch> GetCandidateTargetBranches(Branch branch)
        {
            yield return branch.TrackedBranch;
            yield return repository.Branches.FirstOrDefault(x => x.FriendlyName == "origin/" + branch.FriendlyName);
            yield return repository.Branches.FirstOrDefault(x => x.FriendlyName == "origin/master");
        }
    }
}