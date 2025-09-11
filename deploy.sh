#!/usr/bin/env bash

set -euo pipefail # Catches errors and makes the script exit if unchecked

export LOG_FORMAT='[testcompany] <color_start><level_short><color_end>[<ts_since>][<color_start><script><color_end>] <message>'
export LOG_LEVEL="INFO"

# shellcheck source=/dev/null
# source ./Scripts/log.sh
export VERBOSE=${VERBOSE:-false}
export SPT_DEBUG=${SPT_DEBUG:-false}

if [[ ${SPT_DEBUG} == true ]]; then
  set -x                   # Enables debug for bash script
  export LOG_LEVEL="DEBUG" # Changed log level to debug
fi

declare -r product='Testproduct'
declare -r version='1.0.133'
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
  #depends on
  init

  print_task 'SETUP'

  local startTime
  startTime=$(date +%s)

  print_execution_time "${startTime:?}" "SETUP"
}


function print_task() {
  echo
  echo_color "cyan" "----------------------------------------------------------------------"
  echo_color "cyan" "Executing task: ${1:?}"
  echo_color "cyan" "----------------------------------------------------------------------"
}
