#!/usr/bin/env bash

set -euo pipefail

TEST_RESULTS_XML="$UNITY_DIR/$TEST_PLATFORM-results.xml"
JUNIT_REPORT_XML="$UNITY_DIR/$TEST_PLATFORM-junit-report.xml"
NUNIT_TRANSFORM_XSLT="./ci/nunit-transforms/nunit3-junit.xslt"

# Transform NUnit results to JUnit format using xsltproc
if [ -f "$TEST_RESULTS_XML" ] && [ -f "$NUNIT_TRANSFORM_XSLT" ]; then
    echo "Transforming NUnit results to JUnit format."

    echo "Ensuring xsltproc is installed!"
    apt-get update -q
    apt-get install -y -q xsltproc
    
    xsltproc "$NUNIT_TRANSFORM_XSLT" "$TEST_RESULTS_XML" > "$JUNIT_REPORT_XML"
    echo -e "\033[32JUnit report generated at $JUNIT_REPORT_XML\033[0m"
else
    echo -e "\033[33WARNING: Test results XML ($TEST_RESULTS_XML) or NUnit transform ($NUNIT_TRANSFORM_XSLT) not found. Skipping transformation.\033[0m"
fi
