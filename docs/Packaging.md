
# Packaging

This is the documentation for how the package build process should work.

1. Write `manifest.apkg.json`.
2. It has info about how to build the package. Run the build command. The manifest
   can also have info which files are supposed to be `share` files (they can be
   generated at build time).
3. `apkg` puts everything into a build folder.
4. Info file is generated in the build folder.
5. Build folder is compressed and distributed as an apkg package archive
   (`.lcpkg` file).
6. Apkg package archive is installed with apkg.

