#!/usr/bin/env bash
set -euo pipefail

mode="${1:?Informe resolve ou version.}"
tag="${PREVIEW_DRAFT_TAG:-}"
output="${GITHUB_OUTPUT:?Informe GITHUB_OUTPUT.}"

if [[ "$mode" == resolve && -z "$tag" ]]; then
  [[ "${GITHUB_EVENT_NAME:?}" == push ]] || { echo "::error::Uma tag é obrigatória para o preview manual." >&2; exit 1; }
  printf 'commit=%s\n' "${GITHUB_SHA:?}" >> "$output"
  exit 0
fi

if [[ ! "$tag" =~ ^v(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$ ]]; then
  echo "::error::Informe uma tag estável no formato vX.Y.Z." >&2
  exit 1
fi

case "$mode" in
  resolve)
    if [[ "${GITHUB_EVENT_NAME:?}" != workflow_dispatch || "${GITHUB_REF:?}" != refs/heads/main ]]; then
      echo "::error::O preview de uma release em rascunho exige um disparo manual na main." >&2
      exit 1
    fi
    repository="${GITHUB_REPOSITORY:?}"
    releases="$(gh api --paginate --slurp "repos/$repository/releases?per_page=100")"
    release="$(jq -cer --arg tag "$tag" \
      '[.[][] | select(.tag_name == $tag)] | if length == 1 then .[0] else error("Release ausente ou duplicada") end' <<<"$releases")"
    if ! jq -e '.draft == true and .prerelease == false' <<<"$release" >/dev/null; then
      echo "::error::A release $tag deve ser um rascunho estável." >&2
      exit 1
    fi
    if ! commit="$(git rev-parse --verify "refs/tags/$tag^{commit}")"; then
      echo "::error::A tag $tag não existe no repositório." >&2
      exit 1
    fi
    if ! git merge-base --is-ancestor "$commit" "${GITHUB_SHA:?}"; then
      echo "::error::A tag $tag não é ancestral da main." >&2
      exit 1
    fi
    if ! git diff --quiet "$commit" "$GITHUB_SHA" -- . \
      ':(exclude).github/workflows/**' ':(exclude)docs/**' ':(exclude)tools/scripts/**'; then
      echo "::error::Os arquivos de produção ou teste da main diferem da tag $tag." >&2
      exit 1
    fi
    echo "Os arquivos de produção e teste da main correspondem à tag $tag."
    printf 'commit=%s\n' "$GITHUB_SHA" >> "$output"
    ;;
  version)
    run_number="${GITHUB_RUN_NUMBER:?}"
    if [[ ! "$run_number" =~ ^[1-9][0-9]*$ ]]; then
      echo "::error::Número de execução inválido." >&2
      exit 1
    fi
    script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
    base_output="$(mktemp)"
    trap 'rm -f -- "$base_output"' EXIT
    if ! base_version="$(pwsh "$script_dir/ci/Get-BuildVersion.ps1" -Channel release -Tag "$tag" \
      -RunNumber "$run_number" -OutputPath "$base_output")"; then
      printf '%s\n' "$base_version" >&2
      exit 1
    fi
    prefix="$(awk -F= '$1 == "version_prefix" {print $2}' "$base_output")"
    assembly="$(awk -F= '$1 == "assembly_version" {print $2}' "$base_output")"
    [[ "$prefix" == "${tag#v}" && -n "$assembly" ]] || { echo "::error::Versão base inconsistente." >&2; exit 1; }
    {
      printf 'version=%s-preview.%s\n' "$prefix" "$run_number"
      printf 'version_prefix=%s\nversion_suffix=preview.%s\n' "$prefix" "$run_number"
      printf 'assembly_version=%s\nfile_version=%s.%s\nfile_revision=%s\n' "$assembly" "$prefix" "$run_number" "$run_number"
    } | tee -a "$output"
    ;;
  *)
    echo "::error::Modo inválido: $mode." >&2
    exit 1
    ;;
esac
