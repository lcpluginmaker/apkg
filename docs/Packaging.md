
# Developing and packaging plugins

This is the documentation for how the package build process should work. For
more info about the APIs and how to write your plugins refer to the
[LeoConsole wiki](https://github.com/BoettcherDasOriginal/LeoConsole/wiki).

1. Create your plugin repository. It is recommended to use the
   [template](https://github.com/alexcoder04/LeoConsole-PluginTemplate).
1. Write/edit your `manifest.json`.
2. The manifest contains info about how to build the package. Build the package
   archive using [apkg-builder](https://github.com/alexcoder04/LeoConsole-apkg-builder)
   or the `build` command in apkg (requires activating debug mode).
3. Apkg-builder compiles your plugin, puts the files into a temporary build
   folder, generates a PKGINFO and compresses everything into a `.lcp` archive
   which can be installed using apkg.
4. You can then add your plugin to the
   [contrib repository](https://github.com/lcpluginmaker/repo-contrib)
   by opening a pull request there.

