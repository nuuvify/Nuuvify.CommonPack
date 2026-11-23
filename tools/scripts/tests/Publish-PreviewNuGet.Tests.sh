#!/usr/bin/env bash
set -euo pipefail

script="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/Publish-PreviewNuGet.sh"
temp_dir="$(mktemp -d)"
trap 'rm -rf -- "$temp_dir"' EXIT
mkdir -p "$temp_dir/bin" "$temp_dir/artifacts/nupkg"

cat > "$temp_dir/bin/dotnet" <<'EOF'
#!/usr/bin/env bash
printf '%s\n' "$*" >> "$DOTNET_CALLS"
if [[ "${DOTNET_FAIL:-}" == "1" ]]; then
  echo "simulated NuGet failure" >&2
  exit 17
fi
EOF
chmod +x "$temp_dir/bin/dotnet"
export PATH="$temp_dir/bin:$PATH"
export DOTNET_CALLS="$temp_dir/calls"
cd "$temp_dir"

if env -u NUGET_API_KEY bash "$script" > output 2>&1; then
  echo "Expected failure without NUGET_API_KEY." >&2
  exit 1
fi
grep -q 'Credencial temporaria ausente' output

export NUGET_API_KEY="test-key"
if bash "$script" > output 2>&1; then
  echo "Expected failure without packages." >&2
  exit 1
fi
grep -q 'Nenhum pacote NuGet' output

touch artifacts/nupkg/first.nupkg artifacts/nupkg/second.nupkg artifacts/nupkg/first.snupkg
bash "$script"
[[ "$(wc -l < "$DOTNET_CALLS")" -eq 2 ]]
grep -Fxq 'nuget push artifacts/nupkg/first.nupkg --source https://api.nuget.org/v3/index.json --api-key test-key --skip-duplicate' "$DOTNET_CALLS"
grep -Fxq 'nuget push artifacts/nupkg/second.nupkg --source https://api.nuget.org/v3/index.json --api-key test-key --skip-duplicate' "$DOTNET_CALLS"

: > "$DOTNET_CALLS"
export DOTNET_FAIL=1
if bash "$script" > output 2>&1; then
  echo "Expected failure when NuGet rejects a package." >&2
  exit 1
fi
grep -q 'simulated NuGet failure' output
[[ "$(wc -l < "$DOTNET_CALLS")" -eq 1 ]]
echo "Preview NuGet publication tests passed."
