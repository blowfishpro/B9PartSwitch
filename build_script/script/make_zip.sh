#!/usr/bin/env sh

mkdir -v release
mkdir -v zip_release
cp -v -R GameData release
mkdir -p "release/GameData/${PROJECT_NAME}/Plugins"
cp -v "${PROJECT_DIR}/bin/Release/${PROJECT_NAME}.dll" "release/GameData/${PROJECT_NAME}/Plugins/"
cp -v README.md release
cp -v LICENSE release
cp -v README.md "release/GameData/${PROJECT_NAME}"
cp -v LICENSE "release/GameData/${PROJECT_NAME}"
(cd release && zip -v -r ../zip_release/release.zip *)
