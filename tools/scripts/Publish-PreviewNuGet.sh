#!/usr/bin/env bash
set -euo pipefail

if [[ -z "${NUGET_API_KEY:-}" ]]; then
  echo "::error::Configure NUGET_API_KEY no Environment preview."
  exit 1
fi

shopt -s nullglob
packages=(artifacts/nupkg/*.nupkg)
if (( ${#packages[@]} == 0 )); then
  echo "::error::Nenhum pacote NuGet encontrado para publicação."
  exit 1
fi

for package in "${packages[@]}"; do
  dotnet nuget push "$package" --source https://api.nuget.org/v3/index.json \
    --api-key "$NUGET_API_KEY" --skip-duplicate
done
