# dotnet-guit

A dotnet global tool providing a text-based UI for Git leveraging [gui.cs](https://github.com/migueldeicaza/gui.cs).

[![Version](https://img.shields.io/nuget/vpre/dotnet-guit.svg)](https://www.nuget.org/packages/dotnet-guit)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-guit)](https://www.nuget.org/packages/dotnet-guit)
[![Build Status](https://dev.azure.com/kzu/oss/_apis/build/status/guit?branchName=master)](http://build.azdo.io/kzu/oss/27)
[![License](https://img.shields.io/github/license/kzu/guit.svg)](LICENSE)
[![Join the chat at https://gitter.im/kzu/guit](https://badges.gitter.im/kzu/guit.svg)](https://gitter.im/kzu/guit?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

![changes view](https://github.com/kzu/guit/raw/master/docs/img/changes.png)
![sync view](https://github.com/kzu/guit/raw/master/docs/img/sync.png)
![log view](https://github.com/kzu/guit/raw/master/docs/img/log.png)
![cherry-pick view](https://github.com/kzu/guit/raw/master/docs/img/cherry.png)

Install:

```
dotnet tool install -g dotnet-guit
```

Update:

```
dotnet tool update -g dotnet-guit
```


To use the CI version of the tool, append `--no-cache --add-source https://pkg.kzu.io/index.json` to both operations above, 
or run the `install.cmd` or `update.cmd` from this repository.

To run the tool, open a command prompt on git repo root directory and run `guit`. 


You can also install (or update to) a specific version (i.e. for a PR you send) by looking at the version 
numbers from the build runs in the [AzDO build](https://build.azdo.io/kzu/oss/27).
