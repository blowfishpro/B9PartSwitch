#!/usr/bin/env sh

mkdir -v release
mkdir -v zip_release
mkdir -v zip_release/bintray
cp -v -R GameData release
mkdir -p release/GameData/${PROJECT_NAME}/Plugins
cp -v ${PROJECT_DIR}/bin/Release/${PROJECT_NAME}.dll release/GameData/${PROJECT_NAME}/Plugins
cp -v README.md release
cp -v LICENSE release
(cd release && zip -v -r ../zip_release/release.zip *)
cp -v zip_release/release.zip zip_release/bintray/${PROJECT_NAME}_${TRAVIS_BRANCH}_${TRAVIS_BUILD_NUMBER}_$(git describe --tags).zip
