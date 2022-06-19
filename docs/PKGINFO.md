
# Example PKGINFO file found in the package archive

```json
{
    "manifestVersion": 2.0,
    "packageName": "apkg"
    "packageVersion": "1.2.1",
    "packageOS": "win64",
    "compatibleVersions": ["2.0.0"],
    "depends": [],
    "files": [
        "plugins/apkg.dll",
        "share/docs/apkg/Packaging.md"
    ],
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

