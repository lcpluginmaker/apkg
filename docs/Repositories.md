
# Repositories

Apkg installs packages from so-called repositories. A repository is a collection
of packages that belong together (same author, same use case, ...). A repository
is basically a `index.json` file with the metadata and a list of package names
and their according download links. You can activate/deactivate different
repositories in your apkg config as well as running your own repos.

Such an `index.json` file looks like this:

```json
{
    "indexVersion": 1.0,
    "name": "audio-packages",
    "url": "https://leoconsole-repo.example.com/index.json",
    "project": {
        "description": "packages related to audio and sound",
        "maintainer": "Example Person",
        "email": "example.person@example.com",
        "homepage": "https://example.com/lc-repo.html",
        "bugTracker": "https://example.com/lc-repo/bugs-reports.php"
    },
    "packageList": [
        {
            "name": "lc_player",
            "description": "play audio files",
            "version": "1.0.1",
            "os": "any",
            "url": "https://leoconsole-repo.example.com/lc_player-any-1.0.1.lcpkg"
        },
        {
            "name": "mediainfo",
            "description": "display audio file metadata",
            "version": "0.4.5",
            "os": "win64",
            "url": "https://my-apkg-server.website.com/mediainfo-win64-0.4.5.lcpkg"
        },
        {
            "name": "mediainfo",
            "description": "display audio file metadata",
            "version": "0.4.5",
            "os": "lnx64",
            "url": "https://my-apkg-server.website.com/mediainfo-lnx64-0.4.5.lcpkg"
        }
    ]
}
```

## Notes

 - Naming convention for `.lcpkg` files: `pkgname-os-version.lcpkg`
 - Package names cannot contain following characters: `-`, `.`

