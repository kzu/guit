# dotnet git

A dotnet global tool providing a text-based UI for Git leveraging [gui.cs](https://github.com/migueldeicaza/gui.cs).

[![Version](https://img.shields.io/nuget/vpre/dotnet-guit.svg)](https://www.nuget.org/packages/dotnet-guit)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-guit)](https://www.nuget.org/packages/dotnet-guit)
[![Build Status](https://dev.azure.com/kzu/oss/_apis/build/status/guit?branchName=master)](http://build.azdo.io/kzu/oss/27)
[![License](https://img.shields.io/github/license/kzu/guit.svg)](LICENSE)


To install a CI build run:

```
dotnet tool install -g --no-cache --add-source https://kzu.io/index.json dotnet-guit
```

To update to a CI build, run:

```
dotnet tool update -g --no-cache --add-source https://kzu.io/index.json dotnet-guit
```

To run the tool, open a command prompt on the git repo root directory and run `guit` (or `dotnet guit`). 


You can also install (or update to) a specific version (i.e. for a PR you send) by looking at the version 
numbers from the build runs in the [AzDO build](http://build.azdo.io/kzu/oss/27).
