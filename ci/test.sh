#!/usr/bin/env bash

set -euo pipefail

echo "Testing for $TEST_PLATFORM"

CODE_COVERAGE_PACKAGE="com.unity.testtools.codecoverage"
PACKAGE_MANIFEST_PATH="Packages/manifest.json"

# Discover assembly names from .asmdef files, excluding test/editor assemblies
ASSEMBLY_FILTERS=$(find "$UNITY_DIR/Assets" -name "*.asmdef" \
  ! -name "*Tests.asmdef" \
  ! -name "*Editor.asmdef" \
  | xargs -I{} basename {} .asmdef \
  | sed 's/^/+/' \
  | tr '\n' ';' \
  | sed 's/;$//')
echo "Coverage assembly filters: $ASSEMBLY_FILTERS"

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor} \
  -projectPath $UNITY_DIR \
  -runTests \
  -testPlatform $TEST_PLATFORM \
  -testResults $UNITY_DIR/$TEST_PLATFORM-results.xml \
  -logFile /dev/stdout \
  -batchmode \
  -nographics \
  -enableCodeCoverage \
  -coverageResultsPath $UNITY_DIR/$TEST_PLATFORM-coverage \
  -coverageOptions "generateAdditionalMetrics;generateHtmlReport;generateHtmlReportHistory;generateBadgeReport;assemblyFilters:${ASSEMBLY_FILTERS}" \
  -debugCodeOptimization

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

if grep $CODE_COVERAGE_PACKAGE $PACKAGE_MANIFEST_PATH; then
  if [ -f "$UNITY_DIR/$TEST_PLATFORM-coverage/Report/Summary.xml" ]; then
    cat $UNITY_DIR/$TEST_PLATFORM-coverage/Report/Summary.xml | grep Linecoverage
    mv $UNITY_DIR/$TEST_PLATFORM-coverage/$CI_PROJECT_NAME-opencov/*Mode/TestCoverageResults_*.xml $UNITY_DIR/$TEST_PLATFORM-coverage/
    rm -r $UNITY_DIR/$TEST_PLATFORM-coverage/$CI_PROJECT_NAME-opencov/
  else
    echo -e "\033[33mWARNING: Coverage Summary.xml not found, coverage report may not have been generated.\033[0m"
  fi
else
  echo -e "\033[33mCode Coverage package not found in $PACKAGE_MANIFEST_PATH. Please install the package \"Code Coverage\" through Unity's Package Manager to enable coverage reports.\033[0m"
fi

cat $UNITY_DIR/$TEST_PLATFORM-results.xml | grep test-run | grep Passed
exit $UNITY_EXIT_CODE
