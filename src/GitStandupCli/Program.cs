using GitStandupCli.Models;
using LibGit2Sharp;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GitStandupCli
{
    [Command(
        Name = "git-standup",
        FullName = "A simple command-line utility to report commits of Git repositories for daily standups.")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class Program : CommandBase
    {
        private const ConsoleColor TimeReportingColor = ConsoleColor.Gray;
        private const ConsoleColor BranchReportingColor = ConsoleColor.Yellow;
        private const ConsoleColor MessageReportingColor = ConsoleColor.Magenta;

        private readonly IFileSystem _fileSystem;

        [DirectoryExists]
        [Option(CommandOptionType.MultipleValue, Description = "The path to scan.", ShortName = "p", LongName = "path", ValueName = "PATH")]
        public List<string> Paths { get; set; }

        public DateTime Today => DateTimeOffset.UtcNow.LocalDateTime.Date;
        public DateTime PreviousWorkDay
        {
            get
            {
                var now = DateTimeOffset.Now.LocalDateTime.Date;
                do
                {
                    now = now.AddDays(-1);
                }
                while (IsHoliday(now) || IsWeekend(now));

                return now;


                bool IsWeekend(DateTime date)
                {
                    return date.DayOfWeek == DayOfWeek.Saturday ||
                           date.DayOfWeek == DayOfWeek.Sunday;
                }

                bool IsHoliday(DateTime date)
                {
                    return false;
                }
            }
        }

        public static int Main(string[] args)
        {
            var services = new ServiceCollection()
                .AddSingleton<IConsole, PhysicalConsole>()
                .AddSingleton<IFileSystem, FileSystem>()
                .AddSingleton<IReporter>(provider => new ConsoleReporter(provider.GetService<IConsole>()))
                .BuildServiceProvider();

            var app = new CommandLineApplication<Program>
            {
                ThrowOnUnexpectedArgument = false
            };
            app.Conventions
                .UseDefaultConventions()
                .UseConstructorInjection(services);

            return app.Execute(args);
        }

        public static string GetVersion() => typeof(Program)
            .Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            .InformationalVersion;

        public Program(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task<int> OnExecute(CommandLineApplication app, IConsole console)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (Paths == null || Paths.Count == 0)
                Paths = new List<string> { _fileSystem.Directory.GetCurrentDirectory() };


            console.WriteLine(@" _____        _ _          _____ _                  _             ");
            console.WriteLine(@"|  __ \      (_) |        / ____| |                | |            ");
            console.WriteLine(@"| |  | | __ _ _| |_   _  | (___ | |_ __ _ _ __   __| |_   _ _ __  ");
            console.WriteLine(@"| |  | |/ _` | | | | | |  \___ \| __/ _` | '_ \ / _` | | | | '_ \ ");
            console.WriteLine(@"| |__| | (_| | | | |_| |  ____) | || (_| | | | | (_| | |_| | |_) |");
            console.WriteLine(@"|_____/ \__,_|_|_|\__, | |_____/ \__\__,_|_| |_|\__,_|\__,_| .__/ ");
            console.WriteLine(@"                   __/ |                                   | |    ");
            console.WriteLine(@"                  |___/                by lukas möller     |_|    ");

            console.WriteLineEnter($"{PreviousWorkDay:dddd} {PreviousWorkDay:dd.MM.yyyy} - {Today:dddd} {Today:dd.MM.yyyy}", 2);

            foreach (var path in Paths)
            {
                var repoPath = Repository.Discover(path);

                if (!string.IsNullOrEmpty(repoPath))
                {
                    await ScanDirectory(repoPath, console);
                }
                else
                {
                    var subDirectories = _fileSystem.Directory.GetDirectories(path, ".git", SearchOption.AllDirectories);

                    foreach (var subDirectory in subDirectories)
                    {
                        if (Repository.IsValid(subDirectory))
                        {
                            await ScanDirectory(subDirectory, console);
                        }
                    }
                }
            }

            return 0;
        }

        private async Task ScanDirectory(string path, IConsole console)
        {
            var hasChanges = true;

            void NoChanges()
            {
                if (!hasChanges)
                    return;
                hasChanges = false;
            }

            var repository = new Repository(path);
            console.Write(repository.Info.WorkingDirectory.TrimEnd(new[] { '\\', '/' }));

            var commits = (await GetPreviousDayAndTodaysCommits(repository, repository.Config.GetValueOrDefault<string>("user.email"))).ToList();

            if (!commits.Any())
                NoChanges();

            if (!hasChanges)
            {
                console.Write(" ... Nothing to Say", ConsoleColor.Green);
                console.WriteLine();
            }
            else
            {
                console.WriteLine();
                WriteChanges(console, commits);
            }
        }

        private async Task<IEnumerable<CommitWithBranches>> GetPreviousDayAndTodaysCommits(IRepository repository, string user)
        {
            var commits = new List<CommitWithBranches>();
            foreach (var localBranch in repository.Branches.Where(w => !w.IsRemote))
            {
                commits.AddRange(localBranch.Commits
                    .Where(w => w.Committer.Email == user
                                && w.Committer.When.LocalDateTime.Date >= PreviousWorkDay)
                    .Select(s => new CommitWithBranches(s)));

                foreach (var commit in commits.Where(w => localBranch.Commits.Any(a => a.Sha == w.Sha)))
                {
                    commit.AddBranch(localBranch.FriendlyName);
                }

                if (localBranch.IsTracking)
                {
                    try
                    {
                        var remoteBranch = localBranch.TrackedBranch;
                        commits.AddRange(remoteBranch.Commits?
                            .Where(w => w.Committer.Email == user
                                        && w.Committer.When.LocalDateTime.Date >= PreviousWorkDay
                                        && commits.All(a => a.Sha != w.Sha))
                            .Select(s => new CommitWithBranches(s)));

                        foreach (var commit in commits.Where(w => remoteBranch.Commits.Any(a => a.Sha == w.Sha)))
                        {
                            commit.AddBranch(localBranch.FriendlyName);
                        }
                    }
                    catch (Exception e)
                    {
                        // ignored
                    }
                }
            }
            return await Task.FromResult(commits.OrderByDescending(o => o.Commit.Committer.When));
        }

        private void WriteChanges(IConsole console, IEnumerable<CommitWithBranches> summeries)
        {

            foreach (var summery in summeries)
            {
                console.WriteIndent(1);
                //console.Write(summery.Commit.Committer.When.LocalDateTime.Date.Humanize(false, DateTimeOffset.Now.LocalDateTime, CultureInfo.CurrentCulture));
                console.Write($"{summery.Commit.Committer.When.LocalDateTime:dddd HH:mm}", TimeReportingColor);
                console.WriteIndent(1);
                console.Write(summery.Commit.MessageShort, MessageReportingColor);
                console.WriteIndent(1);
                console.WriteLine(string.Join(" ", summery.Branches), BranchReportingColor);
            }
        }
    }
}
