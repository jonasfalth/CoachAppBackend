#!/usr/bin/env bash

set -euo pipefail # Catches errors and makes the script exit if unchecked

export LOG_FORMAT='[testcompany] <color_start><level_short><color_end>[<ts_since>][<color_start><script><color_end>] <message>'
export LOG_LEVEL="INFO"

# shellcheck source=/dev/null
# source ./Scripts/log.sh
export VERBOSE=${VERBOSE:-false}
export SPT_DEBUG=${SPT_DEBUG:-true}

if [[ ${SPT_DEBUG} == true ]]; then
  set -x                   # Enables debug for bash script
  export LOG_LEVEL="DEBUG" # Changed log level to debug
fi

declare -r product='Testproduct'
declare -r version='0.0.135'
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

init() {
  print_task "INIT"

  echo_color "yellow" "Product        : ${product:?}"
  echo_color "yellow" "Version        : ${version:?}"
  echo_color "yellow" "Build number   : ${buildNumber:-}"
  echo_color "yellow" "Build version  : ${buildVersion:?}"
  echo_color "yellow" "Build label    : ${buildLabel:?}"
  echo_color "yellow" "Branch         : ${branch:?}"
  echo_color "yellow" "Deploy to      : ${deployTo:?}"
}

setup() {
  print_task 'SETUP'
  
  local startTime
  startTime=$(date +%s)
  
  print_execution_time "${startTime:?}" "SETUP"

  # 2.3 Restore, build, pack (Release)
  cd ./source/backend
  
  # Handle codeartifact authentication
  aws codeartifact login --tool nuget --domain coachapplication --repository CoachApp --domain-owner 663797381593 --region eu-north-1

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
}

#====================================================================================================================
# COMMON FUNCTIONS   ================================================================================================
#====================================================================================================================

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

function convert_to_absolute_path() {
  convert_path_from_POSIX_to_windows "$(get_abs_filename_POSIX "${1:?}")"
}

get_abs_filename_POSIX() {
  # $1 : relative filename
  echo "$(cd "$(dirname "${1:?}")" && pwd)/$(basename "${1:?}")"
}

#converts from eg. "/c/projects" to "c:\projects"
function convert_path_from_POSIX_to_windows() {
  local pathToConvert=${1:?'A path to convert is required'}
  echo "$pathToConvert" | sed -e 's/^\///' -e 's/\//\\/g' -e 's/^./\0:/'
}

#Stops when file/path do not exists
function assert_path_exists() {
  local type="${1:?'A type is required (-f/-d)'}"
  local path="${2:?'A path is required'}"
  local msg="${3:?'A message is required'}"
  case "${type:?}" in
  -f) [[ ! -f ${path:?} ]] && echo "${msg:?}" && return 1 ;;
  -d) [[ ! -d ${path:?} ]] && echo "${msg:?}" && return 1 ;;
  esac
  return 0
}

function get_xml_value_from_file() {
  local XMLFile=${1:?}
  local XMLEntity=${2:?}
  grep '<DevelopmentServerPort>' <"${XMLFile:?}" | sed "s/.*<${XMLEntity:?}>\(.*\)<\/${XMLEntity:?}>/\1/"
}

function print_execution_time() {
  local startTime="${1:?'A start time is required'}"
  local name="${2:-}"
  echo_color "purple" "\nExecution time on '${name}': $(date -u --date @$(($(date +%s) - startTime)) +%M:%S)\r"
}

# Returns 0 if the given item (needle) is in the given array (haystack); returns 1 otherwise.
function array_contains {
  local -r needle="$1"
  shift
  local -ra haystack=("$@")

  local item
  for item in "${haystack[@]}"; do
    if [[ "$item" == "$needle" ]]; then
      return 0
    fi
  done

  return 1
}

function ask_continue() {
  local yesNo

  echo -n "${1:?'Message is required'} (yes/no) : "
  read -r yesNo
  if [[ "${yesNo:-}" == "yes" ]]; then
    return 0
  else
    return 1
  fi
}

# Always runs
main() {
  echo "CoachAppBackend start --  ${startupProject}"

  # Run task (passed as the first argument)
  local command="${1:?'A command is required'}"
  shift
  "${command:?}" "$@"

  echo ""
  echo_color "green" "--- Script has finished running ---"
}

# Take all argument in specter.sh X Y and send into main.
main "$@"
