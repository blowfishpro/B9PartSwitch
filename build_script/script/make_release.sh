#!/usr/bin/env sh

mkdir -v release
mkdir -v zip_release
cp -v -R GameData release
mkdir -p "release/GameData/${PROJECT_NAME}/Plugins"
cp -v README.md release
cp -v LICENSE release
cp -v README.md "release/GameData/${PROJECT_NAME}"
cp -v LICENSE "release/GameData/${PROJECT_NAME}"
