
# LeoConsole-apkg

[![Release](https://img.shields.io/github/v/release/alexcoder04/LeoConsole-apkg)](https://github.com/alexcoder04/LeoConsole-apkg/releases/latest)
[![Top language](https://img.shields.io/github/languages/top/alexcoder04/LeoConsole-apkg)](https://github.com/alexcoder04/LeoConsole-apkg/search?l=go)
[![License](https://img.shields.io/github/license/alexcoder04/LeoConsole-apkg)](https://github.com/alexcoder04/LeoConsole-apkg/blob/main/LICENSE)
[![Issues](https://img.shields.io/github/issues/alexcoder04/LeoConsole-apkg)](https://github.com/alexcoder04/LeoConsole-apkg/issues)
[![Pull requests](https://img.shields.io/github/issues-pr/alexcoder04/LeoConsole-apkg)](https://github.com/alexcoder04/LeoConsole-apkg/pulls)
[![Commit activity](https://img.shields.io/github/commit-activity/m/alexcoder04/LeoConsole-apkg)](https://github.com/alexcoder04/LeoConsole-apkg/commits/main)
[![Contributors](https://img.shields.io/github/contributors-anon/alexcoder04/LeoConsole-apkg)](https://github.com/alexcoder04/LeoConsole-apkg/graphs/contributors)

Advanced packaging tool (apt lol) for [LeoConsole](https://github.com/BoettcherDasOriginal/LeoConsole)
and its default package manager.

For more info, check out `apkg help`.

## Installation

### LeoConsole v2.x

The next generation of LeoConsole downloads and sets up apkg absolutely
automatically on first start. Lean back and enjoy!

### LeoConsole v1.x

Due to plugin API changes, new apkg releases are no longer
compatible with LeoConsole v1.x. Please update your LeoConsole to the latest
version.

### Bleeding-edge

You can also try out the latest unpublished version (keep in mind, it could be
unstable):

1. Install [apkg-builder](https://github.com/alexcoder04/LeoConsole-apkg-builder)
2. `git clone https://github.com/alexcoder04/LeoConsole-apkg.git && cd LeoConsole-apkg`
3. `apkg-builder "$PWD"`
4. Enable debug mode in apkg (add `debugModeOn` to your `$SAVEPATH/var/apkg/config`)
4. in LeoConsole, type `apkg get-local <apkg-repo>/apkg.lcpkg`

## Naming

The name consists of multiple ideas merged together:

 - LeoConsole's old `pkg`
 - Debian's `apt`
 - Alpine's `apk`

In total it makes `apkg` xD. Have fun with it!

