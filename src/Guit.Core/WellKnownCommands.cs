namespace Guit
{
    public class WellKnownCommands
    {
        public const string Open = nameof(Open);
        public const string ResolveConflicts = nameof(ResolveConflicts);
        public const string View = nameof(View);
        public const string Refresh = nameof(Refresh);
        public const string SelectAll = nameof(SelectAll);
        public const string Filter = nameof(Filter);
        public const string ShowNextView = nameof(ShowNextView);
        public const string ShowPreviousView = nameof(ShowPreviousView);

        public class Changes
        {
            public const string Commit = nameof(Commit);
            public const string Amend = nameof(Amend);
            public const string Revert = nameof(Revert);
        }

        public class Sync
        {
            public const string Fetch = nameof(Fetch);
            public const string Pull = nameof(Pull);
            public const string Push = nameof(Push);
            public const string SwitchCheckout = nameof(SwitchCheckout);
        }

        public class Log
        {
            public const string Reset = nameof(Reset);
        }

        public class CherryPicker
        {
            public const string CherryPick = nameof(CherryPick);
            public const string Fetch = nameof(Fetch);
            public const string Ignore = nameof(Ignore);
            public const string SelectBaseBranch = nameof(SelectBaseBranch);
            public const string Reset = nameof(Reset);
            public const string Push = nameof(Push);
        }
    }
}
