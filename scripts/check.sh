#!/usr/bin/env bash
set -euo pipefail

repository_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
nuget_cache="${NUGET_HTTP_CACHE_PATH:-/tmp/polyvalent-nuget-http}"
npm_cache="${NPM_CONFIG_CACHE:-/tmp/polyvalent-npm-cache}"

cd "$repository_root"
./scripts/check-private-boundary.sh
NUGET_HTTP_CACHE_PATH="$nuget_cache" dotnet restore Polyvalent.slnx
NUGET_HTTP_CACHE_PATH="$nuget_cache" dotnet build Polyvalent.slnx -c Release --no-restore
NUGET_HTTP_CACHE_PATH="$nuget_cache" dotnet format Polyvalent.slnx --verify-no-changes --no-restore
NUGET_HTTP_CACHE_PATH="$nuget_cache" dotnet test Polyvalent.slnx -c Release --no-build
npm --prefix web ci --cache "$npm_cache"
npm --prefix web run check
git diff --check
