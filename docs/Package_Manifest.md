
# Example package manifest file with all possible options

The manifest file tells apkg how to build a package.

```json
{
    "manifestVersion": 4.0,
    "packageName": "apkg",
    "packageVersion": "2.0.0",
    "depends": [],
    "compatibleVersions": ["2.1.0"],
    "build": {
        "folders": ["share/apkg/bin", "share/docs/apkg"],
        "downloads": [
            {
                "url:win64": "https://github.com/alexcoder04/LeoConsole-apkg-builder/releases/latest/download/apkg-build-win64.exe",
                "url:lnx64": "https://github.com/alexcoder04/LeoConsole-apkg-builder/releases/latest/download/apkg-build-lnx64",
                "path:win64": "share/apkg/bin/apkg-build-win64.exe",
                "path:lnx64": "share/apkg/bin/apkg-build-lnx64"
            }
        ],
        "command": "dotnet",
        "args": ["build", "--nologo"],
        "folder": ".",
        "dlls": ["apkg.dll"],
        "share": "./share"
    },
    "project": {
        "maintainer": "alexcoder04",
        "email": "alexcoder04@protonmail.com",
        "homepage": "https://github.com/alexcoder04/LeoConsole-apkg",
        "bugTracker": "https://github.com/alexcoder04/LeoConsole-apkg/issues"
    }
}
```

## Notes

 - `packageName` has to be unique
 - `packageVersion` has to have the following format: `<int>.<int>.<int>`
 - `depends` lists other packages that are required to use this plugin
 - `compatibleVersions` is a list of LeoConsole versions this plugin is compatible with
 - `build`:
   - `folders` are the folders that will be created at build. If they exist, they will be deleted and created again
   - `downloads` is a list of files to download, `path(:OS)` tells where to place the downloaded file. They can be specific to the OS
   - the `dlls` array tells which dll files need to be installed, the `share` string specifies the folder with the files that go into `share`
 - the `project` key specifies some metadata about the project and the maintainer

