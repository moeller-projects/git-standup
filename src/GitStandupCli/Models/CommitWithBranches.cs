using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GitStandupCli.Models
{
    public class CommitWithBranches
    {
        public string Sha => Commit?.Sha;
        public Commit Commit { get; private set; }
        public IReadOnlyCollection<string> Branches => _branches.OrderBy(o => o.Length).ToList().AsReadOnly();
        private readonly HashSet<string> _branches;

        public CommitWithBranches(Commit commit)
        {
            Commit = commit ?? throw new ArgumentNullException(nameof(commit));
            _branches = new HashSet<string>();
        }

        public void AddBranch(string branchName) => _branches.Add(branchName);
    }
}
