
# LeoConsole-apkg

Advanced packaging tool (apt lol) for [LeoConsole](https://github.com/BoettcherDasOriginal/LeoConsole)
and its default package manager.

For more info, check out `apkg help`.

## Installation

### LeoConsole v2.x

The next generation of LeoConsole downloads and sets up apkg absolutely
automatically on first start. Lean back and enjoy!

### LeoConsole v1.x

apkg is available in the default plugin manager. Inside LeoConsole, type

```
pkg update
pkg get apkg
reboot
```

### Bleeding-edge

You can also try out the latest unpublished version (keep in mind, it could be
unstable):

1. Install [apkg-builder](https://github.com/alexcoder04/LeoConsole-apkg-builder)
2. `git clone https://github.com/alexcoder04/LeoConsole-apkg.git && cd LeoConsole-apkg`
3. `apkg-builder "$PWD"`
4. Enable debug mode in apkg (add `debugModeOn` to your `$SAVEPATH/var/apkg/config`)
4. in LeoConsole, type `apkg get-local <apkg-repo>/apkg.lcpkg`

```
apkg get https://github.com/alexcoder04/LeoConsole-apkg.git
reboot
```

## Naming

The name consists of multiple ideas merged together:

 - LeoConsole's old `pkg`
 - Debian's `apt`
 - Alpine's `apk`

In total it makes `apkg` xD. Have fun with it!

