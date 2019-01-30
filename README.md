# git-standup

[![AppVeyor build status][appveyor-badge]](https://ci.appveyor.com/project/jerriep/git-status-cli/branch/master)

[appveyor-badge]: https://img.shields.io/appveyor/ci/jerriep/git-status-cli/master.svg?label=appveyor&style=flat-square

[![NuGet][main-nuget-badge]][main-nuget] [![MyGet][main-myget-badge]][main-myget]

[main-nuget]: https://www.nuget.org/packages/git-standup-cli/
[main-nuget-badge]: https://img.shields.io/nuget/v/git-standup-cli.svg?style=flat-square&label=nuget
[main-myget]: https://www.myget.org/feed/lftkv/package/nuget/git-standup-cli
[main-myget-badge]: https://img.shields.io/www.myget/lftkv/vpre/git-standup-cli.svg?style=flat-square&label=myget

A simple command-line utility to report commits of Git repositories for daily standups

![](screenshot.png)

## Installation

Download and install the [.NET Core 2.1 SDK](https://www.microsoft.com/net/download) or newer. Once installed, run the following command:

```bash
dotnet tool install --global git-standup-cli
```

## Usage

```text
Usage: git-standup [options]

Options:
  --version         Show version information
  -?|-h|--help      Show help information
  -p|--path <PATH>  The path to scan.
```

By default, **git-standup** will scan for git repositories in the current directory and its sub-directories. You can specify an alternate directory to scan by passing the `-p|--path` option. This option can be passed multiple times for scanning multiple directories.
