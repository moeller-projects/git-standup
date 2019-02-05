[![NuGet][main-nuget-badge]][main-nuget]

[main-nuget]: https://www.nuget.org/packages/git-standup-cli/
[main-nuget-badge]: https://img.shields.io/nuget/v/git-standup-cli.svg?style=flat-square&label=nuget

<a href='https://ko-fi.com/V7V1PWL2' target='_blank'><img height='36' style='border:0px;height:36px;' src='https://az743702.vo.msecnd.net/cdn/kofi5.png?v=0' border='0' alt='Buy Me a Coffee at ko-fi.com' /></a>

# git-standup

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
