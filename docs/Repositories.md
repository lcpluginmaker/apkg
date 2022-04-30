
# Additional repositories

Besides the default `pkg` repo, apkg can be configured to install packages from
third-party repositories. This can be useful to have control which exact
packages can be installed or for building own private packages. The repositories
are simple http(s) servers with certain files on them.

The repository structure looks like this:

```text
 |- index.json
 |- prebuilt.json
 |- recipes.json
```

```json
{
    "indexVersion": 1.0,
    "repoName": "audio-packages",
    "pullFrom": "https://lc-repo.example.com/audio",
    "project": {
        "description": "packages related to audio and sound",
        "maintainer": "Example Person",
        "email": "example.person@example.com",
        "homepage": "https://example.com/lc-repo",
        "bugTracker": "https://example.com/lc-repo/bugs-reports"
    }
}
```

