
# Developing and packaging plugins

This is the documentation for how the package build process should work. For
more info about the APIs and how to write your plugins refer to the
[wiki](https://github.com/BoettcherDasOriginal/LeoConsole/wiki).

1. Create your plugin repository. It is recommended to use the
   [template](https://github.com/alexcoder04/LeoConsole-PluginTemplate).
1. Write/edit your `manifest.apkg.json`.
2. It contains info about how to build the package. Build the package archive
   using [apkg-builder](https://github.com/alexcoder04/LeoConsole-apkg-builder)
   (more info coming soon). The manifest specifies which dlls and which shared
   files should go into your package.
3. Apkg-builder puts everything into a temporary build folder.
4. PKGINFO file is generated in the build folder.
5. Build folder is compressed and distributed as an apkg package archive
   (`.lcpkg` file).
6. Apkg package archive can be installed with apkg.

Note: steps 3-5 are done automaticly by apkg-builder.

