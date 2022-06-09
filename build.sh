#!/bin/sh

# set version
version="$(grep '"packageVersion":' manifest.json | cut -d: -f2 | tr -d '", ')"
sed -Ei "s/apkgVersion\\s?=\\s?\".*\"/apkgVersion=\"$version\"/" "./command.cs"

# build plugin
dotnet build --nologo --verbosity quiet || exit 1

# get builder
BUILDER_REPO="https://github.com/alexcoder04/LeoConsole-apkg-builder"
case "$APKG_BUILDER_OS" in
  lnx64) builder_file="apkg-build-lnx64" ;;
  win64) builder_file="apkg-build-win64.exe" ;;
  *) builder_file="apkg-build-lnx64" ;;
esac

mkdir -vp "./share/scripts"
wget -nv -O "./share/scripts/$builder_file" "$BUILDER_REPO/releases/latest/download/$builder_file" || exit 1

# copy docs
mkdir -vp "./share/docs/apkg"
cp -vr "./docs/"*.md "./share/docs/apkg/"

