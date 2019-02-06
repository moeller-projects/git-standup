using System.Linq;
using LibGit2Sharp;

namespace GitStandupCli
{
    public static class RepositoryStatusExtensions
    {
        public static bool HasUnstagedChanges(this RepositoryStatus status)
        {
            return status.Untracked.Any() ||
                   status.Modified.Any() ||
                   status.Missing.Any() ||
                   status.RenamedInWorkDir.Any();
        }
        public static bool HasStagedChanges(this RepositoryStatus status)
        {
            return status.Added.Any() ||
                   status.Staged.Any() ||
                   status.Removed.Any() ||
                   status.RenamedInIndex.Any();
        }
    }
}