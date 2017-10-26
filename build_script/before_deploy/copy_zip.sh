#!/usr/bin/env sh

mkdir -v zip_release/bintray
cp -v zip_release/release.zip zip_release/bintray/${PROJECT_NAME}_${TRAVIS_BRANCH}_${TRAVIS_BUILD_NUMBER}_$(git describe --tags).zip

mkdir -v zip_release/s3
cp -v zip_release/release.zip zip_release/s3/${PROJECT_NAME}_${TRAVIS_BRANCH}_${TRAVIS_BUILD_NUMBER}_$(git describe --tags).zip

if [ ${TRAVIS_TAG} ]; then
  mkdir -v zip_release/github
  cp -v zip_release/release.zip zip_release/github/${PROJECT_NAME}_${TRAVIS_TAG}.zip
fi
