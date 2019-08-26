# dotnet git

A dotnet global tool providing a text-based UI for Git leveraging [gui.cs](https://github.com/migueldeicaza/gui.cs).

[![Version](https://img.shields.io/nuget/vpre/dotnet-git.svg)](https://www.nuget.org/packages/dotnet-git)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-git)](https://www.nuget.org/packages/dotnet-git)
[![Build Status](https://dev.azure.com/kzu/oss/_apis/build/status/dotnet-git?branchName=master)](http://build.azdo.io/kzu/oss/27)
[![License](https://img.shields.io/github/license/kzu/dotnet-git.svg)](LICENSE)


To install a CI build run:

```
dotnet tool install -g --add-source https://kzu.blob.core.windows.net/nuget/index.json dotnet-git
```

To update to a CI build, run:

```
dotnet tool update -g --add-source https://kzu.blob.core.windows.net/nuget/index.json dotnet-git
```

You can also install (or update to) a specific version (i.e. for a PR you send) by looking at the version 
numbers from the build runs in the [AzDO build](http://build.azdo.io/kzu/oss/27).
