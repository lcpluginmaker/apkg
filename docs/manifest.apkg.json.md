
# Example manifest file with all possible options

The manifest file tells apkg how to build a package.

---

Manifest version:

```json
{
    "manifestVersion": 2.0,
```

Your unique package name:

```json
    "packageName": "apkg",
```

How to build the package (the dlls array tells which dll files need to be installed):

```json
    "build": {
        "command": "dotnet",
        "args": ["build"],
        "folder": ".",
        "dlls": ["apkg.dll"],
        "share": "./docs"
    },
```

Optional additional information:

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

