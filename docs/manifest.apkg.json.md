
# Example package manifest file with all possible options

The manifest file tells apkg how to build a package.

```json
{
    "manifestVersion": 2.1,
    "packageName": "apkg",
    "packageVersion": "1.0.0",
    "build": {
        "command": "dotnet",
        "args": ["build"],
        "folder": ".",
        "dlls": ["apkg.dll"],
        "share": "./docs"
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
 - the `dlls` array tells which dll files need to be installed, the `share`
   string specifies the folder with the files that go into `share`
 - the `project` key specifies some metadata about the project and the maintainer

