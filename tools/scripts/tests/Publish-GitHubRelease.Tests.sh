#!/usr/bin/env bash
set -euo pipefail

script="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/Publish-GitHubRelease.sh"
temp_dir="$(mktemp -d)"
trap 'rm -rf -- "$temp_dir"' EXIT
mkdir -p "$temp_dir/bin" "$temp_dir/artifacts"
export TEST_STATE="$temp_dir/release.json" TEST_LOG="$temp_dir/log"
export TEST_PACKAGE="$temp_dir/package" TEST_SYMBOLS="$temp_dir/symbols"
export PATH="$temp_dir/bin:$PATH" GH_TOKEN=test GITHUB_REPOSITORY=example/repo
export RELEASE_COMMIT=0123456789abcdef0123456789abcdef01234567
export RELEASE_TAG=v3.0.1-preview.94 ARTIFACTS_PATH="$temp_dir/artifacts"
echo null > "$TEST_STATE"

cat > "$temp_dir/bin/gh" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
printf '%s\n' "$*" >> "$TEST_LOG"
if [[ "$1" == api ]]; then
  if [[ "$*" == *matching-refs* ]]; then
    if [[ "${TEST_CONFLICTING_TAG:-}" == 1 ]]; then
      jq -n --arg ref "refs/tags/$RELEASE_TAG" '[{ref:$ref}]'
    else
      printf '[]\n'
    fi
  elif [[ "$*" == *"/commits/"* ]]; then
    printf 'ffffffffffffffffffffffffffffffffffffffff\n'
  elif [[ "$*" == *releases?per_page=100* ]]; then
    jq -c 'if . == null then [[]] else [[.]] end' "$TEST_STATE"
  else
    exit 1
  fi
elif [[ "$1 $2" == 'release create' ]]; then
  jq -n --arg tag "$RELEASE_TAG" --arg commit "$RELEASE_COMMIT" \
    '{tag_name:$tag,target_commitish:$commit,prerelease:true,draft:true,assets:[]}' > "$TEST_STATE"
elif [[ "$1 $2" == 'release upload' ]]; then
  shift 3
  while [[ "$1" != --repo ]]; do
    file="$1"
    digest="sha256:$(sha256sum "$file" | cut -d ' ' -f 1)"
    jq --arg name "${file##*/}" --arg digest "$digest" \
      '.assets += [{name:$name,digest:$digest}]' "$TEST_STATE" > "$TEST_STATE.tmp"
    mv "$TEST_STATE.tmp" "$TEST_STATE"
    shift
  done
elif [[ "$1 $2" == 'release edit' ]]; then
  jq '.draft = false' "$TEST_STATE" > "$TEST_STATE.tmp"
  mv "$TEST_STATE.tmp" "$TEST_STATE"
else
  exit 1
fi
EOF
chmod +x "$temp_dir/bin/gh"
cat > "$temp_dir/bin/curl" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
url=''
output=''
while (( $# )); do
  case "$1" in
    https://*) url="$1" ;;
    -o|--output) shift; output="$1" ;;
  esac
  shift
done
if [[ "$url" == *symbolpackage* ]]; then
  cp "$TEST_SYMBOLS" "$output"
else
  cp "$TEST_PACKAGE" "$output"
fi
EOF
chmod +x "$temp_dir/bin/curl"
printf 'package' > "$TEST_PACKAGE"
printf 'symbols' > "$TEST_SYMBOLS"
printf 'package' > "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1-preview.94.nupkg"
printf 'symbols' > "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1-preview.94.snupkg"

bash "$script" preview
jq -e '.draft == false and .prerelease == true and (.assets | length == 2)' "$TEST_STATE" >/dev/null
uploads="$(grep -c '^release upload' "$TEST_LOG")"
bash "$script" preview
[[ "$(grep -c '^release upload' "$TEST_LOG")" == "$uploads" ]]

export TEST_CONFLICTING_TAG=1
if bash "$script" preview > "$temp_dir/output" 2>&1; then
  echo "Expected failure when the tag points to another commit." >&2
  exit 1
fi
grep -q 'aponta para outro commit' "$temp_dir/output"
unset TEST_CONFLICTING_TAG

jq '.assets[0].digest = "sha256:different"' "$TEST_STATE" > "$TEST_STATE.tmp"
mv "$TEST_STATE.tmp" "$TEST_STATE"
if bash "$script" preview > "$temp_dir/output" 2>&1; then
  echo "Expected failure when an existing asset differs." >&2
  exit 1
fi
grep -q 'digest diferente' "$temp_dir/output"
[[ "$(grep -c '^release upload' "$TEST_LOG")" == "$uploads" ]]

jq '.assets = []' "$TEST_STATE" > "$TEST_STATE.tmp"
mv "$TEST_STATE.tmp" "$TEST_STATE"
if bash "$script" preview > "$temp_dir/output" 2>&1; then
  echo "Expected failure for missing assets in a published release." >&2
  exit 1
fi
grep -q 'Release já publicada' "$temp_dir/output"

export RELEASE_TAG=v3.0.1 RELEASE_ENTRIES="$temp_dir/entries.json"
printf 'package' > "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1.nupkg"
printf 'symbols' > "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1.snupkg"
digest="sha256:$(sha256sum "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1.nupkg" | cut -d ' ' -f 1)"
jq -n --arg digest "$digest" '[{kind:"nuget",name:"Nuuvify.CommonPack.Test",reference:"nugetorg/Nuuvify.CommonPack.Test@3.0.1",digest:$digest}]' > "$RELEASE_ENTRIES"
jq -n --arg tag "$RELEASE_TAG" '{tag_name:$tag,draft:true,prerelease:false,assets:[]}' > "$TEST_STATE"
bash "$script" release
jq -e '.draft == true and (.assets | length == 2)' "$TEST_STATE" >/dev/null
bash "$script" release

jq '.assets = []' "$TEST_STATE" > "$TEST_STATE.tmp"
mv "$TEST_STATE.tmp" "$TEST_STATE"
printf 'rebuilt package' > "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1.nupkg"
bash "$script" release
cmp "$TEST_PACKAGE" "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1.nupkg"
jq -e '(.assets | length == 2)' "$TEST_STATE" >/dev/null
jq -e --arg digest "$digest" '.[0].digest == $digest' "$RELEASE_ENTRIES" >/dev/null

echo "GitHub Release publication tests passed."
