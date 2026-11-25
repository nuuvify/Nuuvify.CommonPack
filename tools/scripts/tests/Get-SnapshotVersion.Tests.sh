#!/usr/bin/env bash
set -euo pipefail

script="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)/ci/Get-SnapshotVersion.ps1"
temp_dir="$(mktemp -d)"
trap 'rm -rf -- "$temp_dir"' EXIT

create_repo() {
  local name="$1" version="$2"
  local repo="$temp_dir/$name"
  mkdir -p "$repo/.github" "$repo/src"
  printf '{"versionFiles":["src/Directory.Build.props"]}\n' > "$repo/.github/ci-cd.json"
  printf '<Project><VersionPrefix>%s</VersionPrefix></Project>\n' "$version" > "$repo/src/Directory.Build.props"
  git -C "$repo" init --quiet --initial-branch=main
  git -C "$repo" config user.name Test
  git -C "$repo" config user.email test@example.invalid
  git -C "$repo" add .
  git -C "$repo" commit --quiet -m initial
  git -C "$repo" tag "v$version"
  printf '%s\n' "$repo"
}

add_commit() {
  local repo="$1" subject="$2" body="${3:-}"
  printf '%s\n' "$subject" > "$repo/change.txt"
  git -C "$repo" add change.txt
  if [[ -n "$body" ]]; then
    printf '%s\n\n%s\n' "$subject" "$body" > "$temp_dir/commit-message"
    git -C "$repo" commit --quiet -F "$temp_dir/commit-message"
  else
    git -C "$repo" commit --quiet -m "$subject"
  fi
}

expect_version() {
  local repo="$1" expected="$2" output="$temp_dir/output"
  : > "$output"
  (cd "$repo" && pwsh "$script" -RunNumber 42 -OutputPath "$output")
  grep -Fxq 'publish_snapshot=true' "$output"
  grep -Fxq "version=$expected-preview.42" "$output"
}

expect_skip() {
  local repo="$1" output="$temp_dir/output"
  : > "$output"
  (cd "$repo" && pwsh "$script" -RunNumber 42 -OutputPath "$output")
  grep -Fxq 'publish_snapshot=false' "$output"
  ! grep -q '^version=' "$output"
}

repo="$(create_repo feat 1.2.3)"
add_commit "$repo" 'feat(email): add tls config'
expect_version "$repo" 1.3.0

repo="$(create_repo patch 1.2.3)"
add_commit "$repo" 'fix(email): handle timeout'
expect_version "$repo" 1.2.4

repo="$(create_repo breaking 1.2.3)"
add_commit "$repo" 'fix(api)!: remove legacy contract'
expect_version "$repo" 2.0.0

repo="$(create_repo breaking-footer 1.2.3)"
add_commit "$repo" 'fix(api): remove legacy contract' 'BREAKING CHANGE: remove the old contract'
expect_version "$repo" 2.0.0

repo="$(create_repo pre-major-breaking 0.2.0)"
add_commit "$repo" 'feat(api)!: revise contract'
expect_version "$repo" 0.3.0

repo="$(create_repo max-bump 1.2.3)"
add_commit "$repo" 'fix(api): correct response'
add_commit "$repo" 'feat(email): add tls config'
expect_version "$repo" 1.3.0

repo="$(create_repo docs 1.2.3)"
add_commit "$repo" 'docs: clarify setup'
expect_skip "$repo"

repo="$(create_repo feature-then-docs 1.2.3)"
add_commit "$repo" 'feat(email): add tls config'
add_commit "$repo" 'docs: clarify setup'
expect_skip "$repo"

repo="$(create_repo release-pr 1.2.3)"
printf '<Project><VersionPrefix>1.3.0</VersionPrefix></Project>\n' > "$repo/src/Directory.Build.props"
git -C "$repo" add src/Directory.Build.props
git -C "$repo" commit --quiet -m 'chore(main): release 1.3.0 (#12)'
expect_skip "$repo"

repo="$(create_repo release-as 1.2.3)"
add_commit "$repo" 'fix(api): align release' 'Release-As: 1.4.0'
expect_version "$repo" 1.4.0

echo "Snapshot version tests passed."
