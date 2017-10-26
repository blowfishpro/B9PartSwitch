#!/usr/bin/env sh

mkdir -v deploy/bintray
cp -v zip_release/release.zip deploy/bintray/${PROJECT_NAME}_${TRAVIS_BRANCH}_${TRAVIS_BUILD_NUMBER}_$(git describe --tags).zip

mkdir -v deploy/s3
cp -v zip_release/release.zip deploy/s3/${PROJECT_NAME}_${TRAVIS_BRANCH}_${TRAVIS_BUILD_NUMBER}_$(git describe --tags).zip

if [ ${TRAVIS_TAG} ]; then
  mkdir -v deploy/github
  cp -v zip_release/release.zip deploy/github/${PROJECT_NAME}_${TRAVIS_TAG}.zip
fi
