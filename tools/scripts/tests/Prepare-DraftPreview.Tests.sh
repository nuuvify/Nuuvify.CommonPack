#!/usr/bin/env bash
set -euo pipefail

script="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/Prepare-DraftPreview.sh"
temp_dir="$(mktemp -d)"
trap 'rm -rf -- "$temp_dir"' EXIT
mkdir -p "$temp_dir/bin" "$temp_dir/repo/.github" "$temp_dir/repo/src"
export PATH="$temp_dir/bin:$PATH" GH_TOKEN=test GITHUB_REPOSITORY=example/repo
export GITHUB_OUTPUT="$temp_dir/output" TEST_RELEASES="$temp_dir/releases.json"
export GITHUB_EVENT_NAME=workflow_dispatch GITHUB_REF=refs/heads/main GITHUB_RUN_NUMBER=112
export PREVIEW_DRAFT_TAG=v3.0.0

cat > "$temp_dir/bin/gh" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
[[ "$*" == "api --paginate --slurp repos/$GITHUB_REPOSITORY/releases?per_page=100" ]]
[[ "${TEST_GH_FAIL:-}" != 1 ]] || exit 1
cat "$TEST_RELEASES"
EOF
chmod +x "$temp_dir/bin/gh"
printf '{"versionFiles":["src/Directory.Build.props"]}\n' > "$temp_dir/repo/.github/ci-cd.json"
printf '<Project><VersionPrefix>3.0.0</VersionPrefix></Project>\n' > "$temp_dir/repo/src/Directory.Build.props"
git init --initial-branch=main --quiet "$temp_dir/repo"
git -C "$temp_dir/repo" -c user.name=Test -c user.email=test@example.invalid add .
git -C "$temp_dir/repo" -c user.name=Test -c user.email=test@example.invalid commit --quiet -m initial
git -C "$temp_dir/repo" tag v3.0.0
mkdir -p "$temp_dir/repo/docs" "$temp_dir/repo/.github/workflows" "$temp_dir/repo/tools/scripts"
printf 'documentation\n' > "$temp_dir/repo/docs/notes.md"
printf 'workflow\n' > "$temp_dir/repo/.github/workflows/ci.yml"
printf 'script\n' > "$temp_dir/repo/tools/scripts/preview.sh"
git -C "$temp_dir/repo" add .
git -C "$temp_dir/repo" -c user.name=Test -c user.email=test@example.invalid commit --quiet -m main
export GITHUB_SHA
GITHUB_SHA="$(git -C "$temp_dir/repo" rev-parse HEAD)"
cd "$temp_dir/repo"

expect_failure() {
  : > "$GITHUB_OUTPUT"
  if "$@" > "$temp_dir/log" 2>&1; then
    echo "Expected failure: $*" >&2
    exit 1
  fi
  if [[ -s "$GITHUB_OUTPUT" ]]; then
    echo "A failed operation must not produce outputs." >&2
    exit 1
  fi
}

printf '[[{"tag_name":"v3.0.0","draft":true,"prerelease":false}]]\n' > "$TEST_RELEASES"
: > "$GITHUB_OUTPUT"
bash "$script" resolve
[[ ! -s "$GITHUB_OUTPUT" ]]

: > "$GITHUB_OUTPUT"
bash "$script" version
grep -Fxq 'version=3.0.0-preview.112' "$GITHUB_OUTPUT"
grep -Fxq 'version_prefix=3.0.0' "$GITHUB_OUTPUT"
grep -Fxq 'version_suffix=preview.112' "$GITHUB_OUTPUT"
grep -Fxq 'assembly_version=3.0.0.0' "$GITHUB_OUTPUT"
grep -Fxq 'file_version=3.0.0.112' "$GITHUB_OUTPUT"
grep -Fxq 'file_revision=112' "$GITHUB_OUTPUT"

pwsh "$(dirname "$script")/ci/Get-BuildVersion.ps1" -Channel snapshot -RunNumber 112 -OutputPath "$temp_dir/snapshot"
grep -Fxq 'version=3.0.1-preview.112' "$temp_dir/snapshot"

expect_failure env PREVIEW_DRAFT_TAG='' bash "$script" resolve
expect_failure env GITHUB_EVENT_NAME=push bash "$script" resolve
expect_failure env GITHUB_REF=refs/heads/other bash "$script" resolve
expect_failure env PREVIEW_DRAFT_TAG='v3.0.0;invalid' bash "$script" resolve
expect_failure env TEST_GH_FAIL=1 bash "$script" resolve
expect_failure env PREVIEW_DRAFT_TAG=v3.0.1 bash "$script" version
expect_failure env GITHUB_RUN_NUMBER=0 bash "$script" version
expect_failure env GITHUB_RUN_NUMBER=65536 bash "$script" version

for release in \
  '[]' \
  '[{"tag_name":"v3.0.0","draft":false,"prerelease":false}]' \
  '[{"tag_name":"v3.0.0","draft":true,"prerelease":true}]' \
  '[{"tag_name":"v3.0.0","draft":true,"prerelease":false},{"tag_name":"v3.0.0","draft":true,"prerelease":false}]'; do
  printf '[%s]\n' "$release" > "$TEST_RELEASES"
  expect_failure bash "$script" resolve
done

printf '[[{"tag_name":"v3.0.2","draft":true,"prerelease":false}]]\n' > "$TEST_RELEASES"
expect_failure env PREVIEW_DRAFT_TAG=v3.0.2 bash "$script" resolve
tree="$(git write-tree)"
other_commit="$(git -c user.name=Test -c user.email=test@example.invalid commit-tree "$tree" -m unrelated)"
git tag v2.9.9 "$other_commit"
printf '[[{"tag_name":"v2.9.9","draft":true,"prerelease":false}]]\n' > "$TEST_RELEASES"
expect_failure env PREVIEW_DRAFT_TAG=v2.9.9 bash "$script" resolve

printf 'changed production code\n' > src/changed.cs
git add src/changed.cs
git -c user.name=Test -c user.email=test@example.invalid commit --quiet -m changed
changed_commit="$(git rev-parse HEAD)"
printf '[[{"tag_name":"v3.0.0","draft":true,"prerelease":false}]]\n' > "$TEST_RELEASES"
expect_failure env GITHUB_SHA="$changed_commit" bash "$script" resolve

echo "Draft preview preparation tests passed."
