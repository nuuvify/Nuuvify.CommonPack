#!/usr/bin/env bash
set -euo pipefail

script="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/Publish-GitHubRelease.sh"
preview_script="$(dirname "$script")/Publish-PreviewNuGet.sh"
repo_root="$(cd "$(dirname "$script")/../.." && pwd)"
ci_workflow="$repo_root/.github/workflows/ci.yml"
prepare_line="$(awk '/run: bash \.\/tools\/scripts\/Publish-GitHubRelease\.sh prepare-preview/ {print NR}' "$ci_workflow")"
publish_line="$(awk '/run: bash \.\/tools\/scripts\/Publish-PreviewNuGet\.sh/ {print NR}' "$ci_workflow")"
attach_line="$(awk '/run: bash \.\/tools\/scripts\/Publish-GitHubRelease\.sh preview/ {print NR}' "$ci_workflow")"
if [[ ! "$prepare_line" =~ ^[0-9]+$ || ! "$publish_line" =~ ^[0-9]+$ || ! "$attach_line" =~ ^[0-9]+$ ]] ||
  (( prepare_line >= publish_line || publish_line >= attach_line )); then
  echo "Expected pre-release creation before NuGet publication and asset upload afterward." >&2
  exit 1
fi
release_workflow="$repo_root/.github/workflows/publish.yml"
verify_line="$(awk '/Publish-GitHubRelease\.sh verify-release/ {print NR}' "$release_workflow")"
stable_publish_line="$(awk '/Publish-NuGet\.ps1 -ArtifactsPath/ {print NR}' "$release_workflow")"
if [[ ! "$verify_line" =~ ^[0-9]+$ || ! "$stable_publish_line" =~ ^[0-9]+$ ]] ||
  (( verify_line >= stable_publish_line )); then
  echo "Expected GitHub Release verification before stable NuGet publication." >&2
  exit 1
fi
temp_dir="$(mktemp -d)"
trap 'rm -rf -- "$temp_dir"' EXIT
mkdir -p "$temp_dir/bin" "$temp_dir/artifacts/nupkg"
export TEST_STATE="$temp_dir/release.json" TEST_LOG="$temp_dir/log" TEST_TAG="$temp_dir/tag"
export TEST_LIST_LAG="$temp_dir/list-lag"
export TEST_PACKAGE="$temp_dir/package" TEST_SYMBOLS="$temp_dir/symbols"
export PATH="$temp_dir/bin:$PATH" GH_TOKEN=test GITHUB_REPOSITORY=example/repo
export RELEASE_COMMIT=0123456789abcdef0123456789abcdef01234567
export RELEASE_TAG=v3.0.1-preview.94 ARTIFACTS_PATH="$temp_dir/artifacts/nupkg"
echo null > "$TEST_STATE"

cat > "$temp_dir/bin/gh" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
printf '%s\n' "$*" >> "$TEST_LOG"
if [[ "$1" == api ]]; then
  if [[ "$*" == *matching-refs* ]]; then
    if [[ -f "$TEST_TAG" ]]; then
      jq -n --arg ref "refs/tags/$RELEASE_TAG" '[{ref:$ref}]'
    else
      printf '[]\n'
    fi
  elif [[ "$*" == *"/commits/"* ]]; then
    if [[ "${TEST_CONFLICTING_TAG:-}" == 1 ]]; then
      printf 'ffffffffffffffffffffffffffffffffffffffff\n'
    else
      cat "$TEST_TAG"
    fi
  elif [[ "$*" == *releases?per_page=100* ]]; then
    if [[ -f "$TEST_LIST_LAG" ]] && (( $(<"$TEST_LIST_LAG") > 0 )); then
      remaining="$(<"$TEST_LIST_LAG")"
      printf '%s\n' "$((remaining - 1))" > "$TEST_LIST_LAG"
      printf '[[]]\n'
    else
      jq -c 'if . == null then [[]] else [[.]] end' "$TEST_STATE"
    fi
  else
    exit 1
  fi
elif [[ "$1 $2" == 'release create' ]]; then
  if [[ "${TEST_CREATE_FAIL:-}" == 1 ]]; then
    echo "simulated release creation failure" >&2
    exit 1
  fi
  prerelease=false
  draft=false
  [[ " $* " != *' --prerelease '* ]] || prerelease=true
  [[ " $* " != *' --draft '* ]] || draft=true
  jq -n --arg tag "$RELEASE_TAG" --arg commit "$RELEASE_COMMIT" \
    --argjson prerelease "$prerelease" --argjson draft "$draft" \
    '{tag_name:$tag,target_commitish:$commit,prerelease:$prerelease,draft:$draft,assets:[]}' > "$TEST_STATE"
  printf '%s\n' "$RELEASE_COMMIT" > "$TEST_TAG"
  printf '%s\n' "${TEST_LIST_DELAY:-0}" > "$TEST_LIST_LAG"
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
else
  exit 1
fi
EOF
chmod +x "$temp_dir/bin/gh"
cat > "$temp_dir/bin/curl" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
printf 'download\n' >> "$TEST_LOG"
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
cat > "$temp_dir/bin/dotnet" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
jq -e --arg tag "$RELEASE_TAG" '.tag_name == $tag and .prerelease == true and .draft == false' "$TEST_STATE" >/dev/null
printf 'nuget-push\n' >> "$TEST_LOG"
EOF
chmod +x "$temp_dir/bin/dotnet"
cat > "$temp_dir/bin/sleep" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
printf 'sleep %s\n' "$1" >> "$TEST_LOG"
EOF
chmod +x "$temp_dir/bin/sleep"
printf 'package' > "$TEST_PACKAGE"
printf 'symbols' > "$TEST_SYMBOLS"
printf 'package' > "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1-preview.94.nupkg"
printf 'symbols' > "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1-preview.94.snupkg"

if bash "$script" preview > "$temp_dir/output" 2>&1; then
  echo "Expected failure when the pre-release is missing." >&2
  exit 1
fi
grep -q 'GitHub Release .* não existe' "$temp_dir/output"
if grep -q '^download$' "$TEST_LOG"; then
  echo "Downloads must not start before the pre-release exists." >&2
  exit 1
fi

export TEST_CREATE_FAIL=1 NUGET_API_KEY=test-key
if (bash "$script" prepare-preview && cd "$temp_dir" && bash "$preview_script") > "$temp_dir/output" 2>&1; then
  echo "Expected failure when the pre-release cannot be created." >&2
  exit 1
fi
if grep -q '^nuget-push$' "$TEST_LOG"; then
  echo "NuGet must not be called after a pre-release creation failure." >&2
  exit 1
fi
unset TEST_CREATE_FAIL

export TEST_LIST_DELAY=99
if (bash "$script" prepare-preview && cd "$temp_dir" && bash "$preview_script") > "$temp_dir/output" 2>&1; then
  echo "Expected failure when the new pre-release cannot be confirmed." >&2
  exit 1
fi
grep -q 'não ficou disponível para confirmação' "$temp_dir/output"
if grep -q '^nuget-push$' "$TEST_LOG"; then
  echo "NuGet must not be called before the pre-release is confirmed." >&2
  exit 1
fi
unset TEST_LIST_DELAY
echo null > "$TEST_STATE"
rm -f -- "$TEST_TAG" "$TEST_LIST_LAG"
: > "$TEST_LOG"

TEST_LIST_DELAY=2 bash "$script" prepare-preview
jq -e '.draft == false and .prerelease == true and (.assets | length == 0)' "$TEST_STATE" >/dev/null
[[ "$(grep -c '^sleep ' "$TEST_LOG")" == 2 ]]
if grep -q '^download$' "$TEST_LOG"; then
  echo "Pre-release creation must not depend on NuGet downloads." >&2
  exit 1
fi
creates="$(grep -c '^release create' "$TEST_LOG")"
bash "$script" prepare-preview
[[ "$(grep -c '^release create' "$TEST_LOG")" == "$creates" ]]
(cd "$temp_dir" && bash "$preview_script")
grep -q '^nuget-push$' "$TEST_LOG"

bash "$script" preview
jq -e '.draft == false and .prerelease == true and (.assets | length == 2)' "$TEST_STATE" >/dev/null
uploads="$(grep -c '^release upload' "$TEST_LOG")"
bash "$script" preview
[[ "$(grep -c '^release upload' "$TEST_LOG")" == "$uploads" ]]

export TEST_CONFLICTING_TAG=1
pushes="$(grep -c '^nuget-push$' "$TEST_LOG")"
if (bash "$script" prepare-preview && cd "$temp_dir" && bash "$preview_script") > "$temp_dir/output" 2>&1; then
  echo "Expected failure when the tag points to another commit." >&2
  exit 1
fi
grep -q 'aponta para outro commit' "$temp_dir/output"
[[ "$(grep -c '^nuget-push$' "$TEST_LOG")" == "$pushes" ]]
unset TEST_CONFLICTING_TAG

jq '.assets[0].digest = "sha256:different"' "$TEST_STATE" > "$TEST_STATE.tmp"
mv "$TEST_STATE.tmp" "$TEST_STATE"
if bash "$script" preview > "$temp_dir/output" 2>&1; then
  echo "Expected failure when an existing asset differs." >&2
  exit 1
fi
grep -q 'digest diferente' "$temp_dir/output"
[[ "$(grep -c '^release upload' "$TEST_LOG")" == "$uploads" ]]

jq '.assets = [.assets[1]]' "$TEST_STATE" > "$TEST_STATE.tmp"
mv "$TEST_STATE.tmp" "$TEST_STATE"
bash "$script" preview
jq -e '(.assets | length == 2)' "$TEST_STATE" >/dev/null

jq '.draft = true' "$TEST_STATE" > "$TEST_STATE.tmp"
mv "$TEST_STATE.tmp" "$TEST_STATE"
if bash "$script" prepare-preview > "$temp_dir/output" 2>&1; then
  echo "Expected failure when the pre-release is still a draft." >&2
  exit 1
fi
grep -q 'pre-release publicada' "$temp_dir/output"

export RELEASE_TAG=v3.0.1 RELEASE_ENTRIES="$temp_dir/entries.json"
printf 'package' > "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1.nupkg"
printf 'symbols' > "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1.snupkg"
digest="sha256:$(sha256sum "$ARTIFACTS_PATH/Nuuvify.CommonPack.Test.3.0.1.nupkg" | cut -d ' ' -f 1)"
jq -n --arg digest "$digest" '[{kind:"nuget",name:"Nuuvify.CommonPack.Test",reference:"nugetorg/Nuuvify.CommonPack.Test@3.0.1",digest:$digest}]' > "$RELEASE_ENTRIES"
echo null > "$TEST_STATE"
if bash "$script" verify-release > "$temp_dir/output" 2>&1; then
  echo "Expected stable publication to fail without a GitHub Release." >&2
  exit 1
fi
grep -q 'GitHub Release .* não existe' "$temp_dir/output"
jq -n --arg tag "$RELEASE_TAG" '{tag_name:$tag,draft:true,prerelease:false,assets:[]}' > "$TEST_STATE"
bash "$script" verify-release
jq '.prerelease = true' "$TEST_STATE" > "$TEST_STATE.tmp"
mv "$TEST_STATE.tmp" "$TEST_STATE"
if bash "$script" verify-release > "$temp_dir/output" 2>&1; then
  echo "Expected failure when the stable GitHub Release is a pre-release." >&2
  exit 1
fi
jq '.prerelease = false' "$TEST_STATE" > "$TEST_STATE.tmp"
mv "$TEST_STATE.tmp" "$TEST_STATE"
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

jq '.draft = false | .assets = []' "$TEST_STATE" > "$TEST_STATE.tmp"
mv "$TEST_STATE.tmp" "$TEST_STATE"
if bash "$script" release > "$temp_dir/output" 2>&1; then
  echo "Expected failure for missing assets in a published stable release." >&2
  exit 1
fi
grep -q 'Release já publicada' "$temp_dir/output"

echo "GitHub Release publication tests passed."
