#!/usr/bin/env bash
set -euo pipefail

mode="${1:?Informe prepare-preview, preview, verify-release ou release.}"
tag="${RELEASE_TAG:?Informe RELEASE_TAG.}"
commit="${RELEASE_COMMIT:?Informe RELEASE_COMMIT.}"
repository="${GITHUB_REPOSITORY:?Informe GITHUB_REPOSITORY.}"
artifacts="${ARTIFACTS_PATH:-artifacts/nupkg}"
version="${tag#v}"

if [[ ! "$mode" =~ ^(prepare-preview|preview|verify-release|release)$ || ! "$tag" =~ ^v[0-9]+\.[0-9]+\.[0-9]+(-preview\.[0-9]+)?$ || ! "$commit" =~ ^[0-9a-f]{40}$ ]]; then
  echo "::error::Modo, tag ou commit inválido."
  exit 1
fi
if [[ "$mode" =~ ^(prepare-preview|preview)$ && ! "$version" =~ -preview\.[0-9]+$ ]] ||
  [[ "$mode" =~ ^(verify-release|release)$ && "$version" == *-* ]]; then
  echo "::error::A tag não corresponde ao canal informado."
  exit 1
fi

verify_tag() {
  local required="$1" refs actual_commit
  refs="$(gh api "repos/$repository/git/matching-refs/tags/$tag")"
  if jq -e --arg ref "refs/tags/$tag" 'any(.[]; .ref == $ref)' <<<"$refs" >/dev/null; then
    actual_commit="$(gh api "repos/$repository/commits/$tag" --jq .sha)"
    if [[ "$actual_commit" != "$commit" ]]; then
      echo "::error::A tag $tag aponta para outro commit."
      exit 1
    fi
  elif [[ "$required" == true ]]; then
    echo "::error::A tag $tag não existe."
    exit 1
  fi
}

find_release() {
  gh api --paginate --slurp "repos/$repository/releases?per_page=100" |
    jq -c --arg tag "$tag" '[.[][] | select(.tag_name == $tag)] | if length > 1 then error("Release duplicada") else .[0] // null end'
}

verify_tag false
release="$(find_release)"
if [[ "$release" == null ]]; then
  if [[ "$mode" != prepare-preview ]]; then
    echo "::error::A GitHub Release $tag não existe."
    exit 1
  fi
  gh release create "$tag" --repo "$repository" --target "$commit" --prerelease \
    --title "$tag" --notes "Preview do commit $commit. Os pacotes serão anexados após a publicação e indexação no NuGet.org. Consulte o CI para verificar o resultado."
  for attempt in {1..12}; do
    release="$(find_release)"
    if jq -e '.prerelease == true and .draft == false' <<<"$release" >/dev/null; then
      break
    fi
    if (( attempt == 12 )); then
      echo "::error::A pre-release $tag foi criada, mas não ficou disponível para confirmação. Reexecute o job Snapshot."
      exit 1
    fi
    echo "Aguardando confirmação da pre-release $tag ($attempt/12)."
    sleep 5
  done
fi

verify_tag true
if [[ "$mode" =~ ^(prepare-preview|preview)$ ]] && ! jq -e '.prerelease == true and .draft == false' <<<"$release" >/dev/null; then
  echo "::error::A release $tag deve ser uma pre-release publicada."
  exit 1
elif [[ "$mode" =~ ^(verify-release|release)$ ]] && ! jq -e '.prerelease == false' <<<"$release" >/dev/null; then
  echo "::error::A release $tag deve ser estável."
  exit 1
fi

if [[ "$mode" =~ ^(prepare-preview|verify-release)$ ]]; then
  exit 0
fi

shopt -s nullglob
packages=("$artifacts"/*."$version".nupkg)
if (( ${#packages[@]} == 0 )); then
  echo "::error::Nenhum pacote da versão $version encontrado."
  exit 1
fi

files=()
for package in "${packages[@]}"; do
  name="${package##*/}"
  id="${name%."$version".nupkg}"
  symbols="$artifacts/$id.$version.snupkg"
  if [[ ! -f "$symbols" ]]; then
    echo "::error::Pacote de símbolos ausente: $symbols."
    exit 1
  fi

  lower_id="${id,,}"
  if [[ "$mode" == release ]]; then
    entries="${RELEASE_ENTRIES:?Informe RELEASE_ENTRIES.}"
    jq -e --arg id "$id" --arg version "$version" \
      '[.[] | select(.kind == "nuget" and .name == $id and .reference == ("nugetorg/" + $id + "@" + $version))] | length == 1' \
      "$entries" >/dev/null
  fi

  curl --fail --silent --show-error --location --proto '=https' --proto-redir '=https' --retry 80 --retry-delay 10 --retry-all-errors \
    --retry-max-time 900 --connect-timeout 10 --max-time 30 \
    "https://api.nuget.org/v3-flatcontainer/$lower_id/$version/$lower_id.$version.nupkg" -o "$package.download"
  mv "$package.download" "$package"
  curl --fail --silent --show-error --location --proto '=https' --proto-redir '=https' --retry 80 --retry-delay 10 --retry-all-errors \
    --retry-max-time 900 --connect-timeout 10 --max-time 30 \
    "https://www.nuget.org/api/v2/symbolpackage/$id/$version" -o "$symbols.download"
  mv "$symbols.download" "$symbols"
  if [[ "$mode" == release ]]; then
    digest="sha256:$(sha256sum "$package" | cut -d ' ' -f 1)"
    jq --arg id "$id" --arg digest "$digest" \
      'map(if .kind == "nuget" and .name == $id then .digest = $digest else . end)' \
      "$entries" > "$entries.tmp"
    mv "$entries.tmp" "$entries"
  fi
  files+=("$package" "$symbols")
done

missing=()
for file in "${files[@]}"; do
  name="${file##*/}"
  existing="$(jq -r --arg name "$name" '[.assets[] | select(.name == $name)] | if length > 1 then error("Asset duplicado") elif length == 1 then .[0].digest // "missing-digest" else "" end' <<<"$release")"
  if [[ -z "$existing" ]]; then
    if [[ "$mode" == release && "$(jq -r .draft <<<"$release")" != true ]]; then
      echo "::error::Release já publicada sem o asset $name; não será alterada."
      exit 1
    fi
    missing+=("$file")
  elif [[ "$existing" != "sha256:$(sha256sum "$file" | cut -d ' ' -f 1)" ]]; then
    echo "::error::O asset existente $name tem digest diferente."
    exit 1
  fi
done
if (( ${#missing[@]} )); then
  gh release upload "$tag" "${missing[@]}" --repo "$repository"
fi
