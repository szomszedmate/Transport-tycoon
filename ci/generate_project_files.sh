#!/usr/bin/env bash

set -euo pipefail

echo "Installing .NET"

# Update package lists
apt-get update -q
# Install the .NET SDK, .NET Runtime and NuGet CLI
apt-get install -q -y dotnet-sdk-8.0 dotnet-runtime-8.0 nuget


echo "Generating project files"

${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' unity-editor} \
  -projectPath $UNITY_DIR \
  -quit \
  -batchmode \
  -nographics \
  -executeMethod GenerateSolution.RegenerateProjectFiles \
  -logFile /dev/stdout

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo -e "\033[32Project and solution files successfully generated\033[0m"
  ls *.sln *.csproj 2>/dev/null
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo -e "\033[31Project and solution files failed to generate\033[0m"
else
  echo -e "\033[31Unexpected exit code $UNITY_EXIT_CODE\033[0m"
fi
