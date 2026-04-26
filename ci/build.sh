#!/usr/bin/env bash

set -euo pipefail

echo "Building for $BUILD_TARGET"

export BUILD_PATH=$UNITY_DIR/Builds/$BUILD_TARGET/
mkdir -p $BUILD_PATH

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor} \
  -projectPath $UNITY_DIR \
  -quit \
  -batchmode \
  -nographics \
  -buildTarget $BUILD_TARGET \
  -customBuildTarget $BUILD_TARGET \
  -customBuildName $BUILD_NAME \
  -customBuildPath $BUILD_PATH \
  -executeMethod BuildCommand.PerformBuild \
  -logFile /dev/stdout

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo -e "\033[32mRun succeeded, no failures occurred\033[0m";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo -e "\033[33mRun succeeded, some tests failed\033[0m";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo -e "\033[31mRun failure (other failure)\033[0m";
else
  echo -e "\033[31mUnexpected exit code $UNITY_EXIT_CODE\033[0m";
fi

ls -la $BUILD_PATH
[ -n "$(ls -A $BUILD_PATH)" ] # fail job if build folder is empty
