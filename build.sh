#!/bin/sh

# set version
version="$(grep '"packageVersion":' manifest.json | cut -d: -f2 | tr -d '", ')"
sed -Ei "s/apkgVersion\\s?=\\s?\".*\"/apkgVersion=\"$version\"/" "./command.cs"

# build plugin
dotnet build --nologo --verbosity quiet || exit 1

# copy docs
cp -vr "./docs/"*.md "./share/docs/apkg/" || exit 1

