# dotnet git

A dotnet global tool providing a text-based UI for Git leveraging [gui.cs](https://github.com/migueldeicaza/gui.cs).

[![Version](https://img.shields.io/nuget/vpre/guit.svg)](https://www.nuget.org/packages/guit)
[![Downloads](https://img.shields.io/nuget/dt/guit)](https://www.nuget.org/packages/guit)
[![Build Status](https://dev.azure.com/kzu/oss/_apis/build/status/guit?branchName=master)](http://build.azdo.io/kzu/oss/27)
[![License](https://img.shields.io/github/license/kzu/guit.svg)](LICENSE)
[![Join the chat at https://gitter.im/kzu/guit](https://badges.gitter.im/kzu/guit.svg)](https://gitter.im/kzu/guit?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

Install:

```
dotnet tool install -g guit
```

Update:

```
dotnet tool update -g guit
```


To use the CI version of the tool, append `--no-cache --add-source https://www.kzu.io/index.json` to both operations above, 
or run the `install.cmd` or `update.cmd` from this repository.

To run the tool, open a command prompt on git repo root directory and run `guit` (or `dotnet guit`). 


You can also install (or update to) a specific version (i.e. for a PR you send) by looking at the version 
numbers from the build runs in the [AzDO build](https://build.azdo.io/kzu/oss/27).
