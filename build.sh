#!/bin/sh

dotnet build --nologo
mkdir -vp "./share/docs/apkg"
cp -vr "./docs/"*.md "./share/docs/apkg/"

