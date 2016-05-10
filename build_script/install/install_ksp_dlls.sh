#!/usr/bin/env sh

wget --no-check-certificate --quiet "https://drive.google.com/uc?export=download&id=${GDRIVE_DLL_PACKAGE_ID}" -O KSP_DLLs.zip
unzip -P ${DLL_PACKAGE_ZIP_PASSPHRASE} KSP_DLLs.zip
mkdir -v -p ${PROJECT_DIR}/bin/Release/
cp -v KSP_DLLs/${KSP_VERSION}/* ${PROJECT_DIR}/bin/Release/