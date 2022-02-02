
# Example manifest file with all possible options

Manifest version:

```json
{
    "manifestVersion": 1.0,
```

Your unique package name:

```json
    "packageName": "apkg",
```

How to build the package:

```json
    "build": {
        "command": "dotnet",
        "args": "build",
        "folder": "."
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

