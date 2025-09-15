#!/usr/bin/env bash

set -euo pipefail # Catches errors and makes the script exit if unchecked

export LOG_FORMAT='[testcompany] <color_start><level_short><color_end>[<ts_since>][<color_start><script><color_end>] <message>'
export LOG_LEVEL="INFO"

# shellcheck source=/dev/null
# source ./Scripts/log.sh
export VERBOSE=${VERBOSE:-true}
export SPT_DEBUG=${SPT_DEBUG:-true}

if [[ ${SPT_DEBUG} == true ]]; then
  set -x                   # Enables debug for bash script
  export LOG_LEVEL="DEBUG" # Changed log level to debug
fi

declare -r product='Testproduct'
declare -r version='0.0.134'
declare -r company="Testcompany"
declare -r copyright="${company:?} 2022 - $(date +"%Y")"

declare -r rootDir='.'
declare -r sourceDir="${rootDir:?}/Source"
declare -r buildDir="${rootDir:?}/Build"
declare -r artifactsDir="${buildDir:?}/Artifacts"

declare -r startupProject='Test'
declare -r localhostUrlWithPort='https://localhost:44339'

declare artifactsName="${product:?}-${version:?}-release"
declare -r artifactsName="${artifactsName//./_}"

declare branch="${BUILDKITE_BRANCH:-}"
declare buildNumber="${BUILDKITE_BUILD_NUMBER:-}"
declare buildVersion
declare buildLabel
declare deployTo='Local'

print_task "INIT"

echo_color "yellow" "Product        : ${product:?}"
echo_color "yellow" "Version        : ${version:?}"
echo_color "yellow" "Build number   : ${buildNumber:-}"
echo_color "yellow" "Build version  : ${buildVersion:?}"
echo_color "yellow" "Build label    : ${buildLabel:?}"
echo_color "yellow" "Branch         : ${branch:?}"
echo_color "yellow" "Deploy to      : ${deployTo:?}"


print_task 'SETUP'

local startTime
startTime=$(date +%s)

print_execution_time "${startTime:?}" "SETUP"

# Handle codeartifact authentication
aws codeartifact login --tool nuget --domain coachapplication --repository CoachApp --domain-owner 663797381593 --region eu-north-1
Write-Host "PackageVersion = ${$version}"

# 2.3 Restore, build, pack (Release)
cd ./source/backend
dotnet restore
dotnet build -c Release -p:ContinuousIntegrationBuild=true
  
# Packa
dotnet pack ./coach-backend.csproj -c Release -o ./artifacts -p:PackageVersion="$version"
  
# Pusha .nupkg (snupkg är frivilligt – pusha om du vill ha symboler)
sourceName="coachapplication/CoachApp"

# Om inga filer matchar, skippa loopen i stället för att använda bokstavlig sträng
shopt -s nullglob
for pkg in ./artifacts/*.nupkg; do
  dotnet nuget push "$pkg" --source "$sourceName" --skip-duplicate
done


function print_task() {
  echo
  echo_color "cyan" "----------------------------------------------------------------------"
  echo_color "cyan" "Executing task: ${1:?}"
  echo_color "cyan" "----------------------------------------------------------------------"
}

function echo_color() {
  local color=${1:?'A color is required'}
  local colorCode="0;30" #Black is default if no match
  local noColor="\\033[0m"
  shift
  local textToColorize="$*"
  case "${color}" in
  "red") colorCode="\\033[0;31m" ;;
  "green") colorCode="\\033[0;32m" ;;
  "yellow") colorCode="\\033[0;33m" ;;
  "blue") colorCode="\\033[0;34m" ;;
  "purple") colorCode="\\033[0;35m" ;;
  "cyan") colorCode="\\033[0;36m" ;;
  esac
  echo -e "${noColor}${colorCode}${textToColorize}${noColor}"
}

function print_execution_time() {
  local startTime="${1:?'A start time is required'}"
  local name="${2:-}"
  echo_color "purple" "\nExecution time on '${name}': $(date -u --date @$(($(date +%s) - startTime)) +%M:%S)\r"
}
