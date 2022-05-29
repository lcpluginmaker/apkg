#!/bin/sh

version="$(grep '"packageVersion":' manifest.apkg.json | cut -d: -f2 | tr -d '", ')"
sed -Ei "s/apkgVersion\\s?=\\s?\".*\"/apkgVersion=\"$version\"/" "./command.cs"

dotnet build --nologo

mkdir -vp "./share/docs/apkg"
cp -vr "./docs/"*.md "./share/docs/apkg/"

