#!/bin/sh

dotnet build --nologo
mkdir -vp "./share/docs"
cp -vr "./docs" "./share/docs/apkg"

