
# Example package manifest file with all possible options

The manifest file tells apkg how to build a package.

---

Manifest version:

```json
{
    "manifestVersion": 2.1,
```

Your unique package name and version:

```json
    "packageName": "apkg",
    "packageVersion": "1.0.0",
```

How to build the package (the `dlls` array tells which dll files need to be
installed, the `share` string specifies the folder with the files that go into
`share`):

```json
    "build": {
        "command": "dotnet",
        "args": ["build"],
        "folder": ".",
        "dlls": ["apkg.dll"],
        "share": "./docs"
    },
```

Project information:

```json
    "project": {
        "maintainer": "alexcoder04",
        "email": "alexcoder04@protonmail.com",
        "homepage": "https://github.com/alexcoder04/LeoConsole-apkg",
        "bugTracker": "https://github.com/alexcoder04/LeoConsole-apkg/issues"
    }
}
```

---

