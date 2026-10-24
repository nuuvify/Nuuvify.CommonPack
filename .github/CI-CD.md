# CI/CD do Nuuvify CommonPack

Arquivo gerenciado pelo Altivy.Groundwork (componente `ci`). Não edite: mudanças no
processo são feitas no catálogo do Groundwork e chegam ao projeto por `groundwork update`.

## Fluxo

1. Todo PR para `main` executa `.github/workflows/ci.yml`, que chama
   `gw-pr-hygiene.yml` (título Conventional Commit, actionlint e arquivos gerenciados),
   `gw-gitleaks.yml` (histórico completo) e os jobs do projeto.
2. O job `Result` do `ci.yml` agrega todos os jobs obrigatórios e é o **único check
   obrigatório** da branch padrão.
3. O merge é sempre squash: o título do PR vira a mensagem do commit e decide se a mudança
   é releasable.
4. Na branch padrão, `gw-release.yml` executa o release-please com o GitHub App de release:
   commits releasable (`feat`, `fix`, `deps`, `perf`, `revert` ou com `!`) criam ou
   atualizam o Release PR; os demais apenas integram.
5. O merge humano do Release PR cria a tag `vX.Y.Z` e a GitHub Release em rascunho, chama
   `publish.yml` no Environment `release`, gera e atesta o `release-manifest.json`, só
   então publica a release e, com ela pública, chama `post-release.yml`.

## Workflows

| Workflow | Tipo | Função |
| --- | --- | --- |
| `ci.yml` | semente | CI do projeto e job `Result` |
| `gw-pr-hygiene.yml` | gerenciado | título do PR, impacto na release, actionlint e `Test-ManagedFiles.ps1` |
| `gw-gitleaks.yml` | gerenciado | varredura de secrets no histórico completo |
| `gw-dotnet.yml` | gerenciado | restore, build, test e pack .NET na imagem `dotnet.sdkImage` |
| `gw-node.yml` | gerenciado | install, test e build Node em imagem fixada |
| `gw-release.yml` | gerenciado | Release PR, tag, publish, manifesto atestado e auto-deploy opcional |
| `publish.yml` | semente | publicação dos artefatos da release |
| `post-release.yml` | semente | distribuição que exige a release pública (por padrão, nenhuma) |
| `gw-publish-nuget.yml` | gerenciado | pacotes NuGet idempotentes (release e snapshots) |

Arquivos **gerenciados** (`gw-*.yml`, `tools/scripts/ci/*`, este documento e os runbooks
padrão) têm SHA-256 registrado em `.groundwork/manifest.json`; `Test-ManagedFiles.ps1`
reprova o PR que os altera. A regra vale para os componentes de CI/CD; arquivos de `core`,
`secret-scanning` e `speckit` admitem personalização, reconciliada pelo `groundwork update`.
Arquivos **semente** são criados uma vez e pertencem ao projeto.

## Contratos das sementes

- `.github/ci-cd.json`: `defaultBranch`, `commitScopes`, `dotnet.sdkImage`, `dotnet.solution`,
  `versionFiles`, `publishSnapshots`, `deploy.environments`, `deploy.autoDeployEnvironment`,
  `attestations` e `rollbackMinimumVersion`. É lido pelos workflows e scripts gerenciados.
- `attestations`: padrão `true`. Use `false` somente quando o plano do GitHub não oferece atestação
  de artefatos (repositório privado fora do Enterprise Cloud). Com `false`, manifesto, imagens e
  assets não são atestados, o manifesto registra `"attestation": "none"` e o guard de deploy recusa
  a release. Campo ausente equivale a `true`; qualquer valor que não seja booleano falha o workflow.
- `ci.yml`: todo job do projeto entra em `needs` do job `Result`; o CI funciona sem secrets,
  pois PRs do Dependabot não os recebem.
- `publish.yml`: `workflow_call` com `tag`, `version` e `republish`; publica no Environment
  `release` e envia artifacts `release-entries-*` com uma lista JSON de
  `{ "kind": "nuget|container|asset", "name", "reference", "digest": "sha256:..." }`.
  Em `republish`, envia somente o que falta e nunca sobrescreve artefato publicado.
- `post-release.yml`: `workflow_call` com `tag`, `version` e `republish`, executado depois que a
  release foi publicada, com `contents: read` e os secrets do repositório. Use-o para distribuição
  que baixa os assets publicados (Homebrew, winget); mantenha os passos idempotentes, pois o
  `republish` o executa de novo.
- `rollbackMinimumVersion`: atualize no mesmo PR que impedir a volta para versões anteriores
  (migração *contract*, remoção de protocolo N-1, formato de dado incompatível).

## Pré-requisitos
- GitHub App de release instalado no repositório, `vars.RELEASE_APP_ID` e o secret
  `RELEASE_APP_PRIVATE_KEY` (criados manualmente com `gh variable set` e `gh secret set`).
  Sem eles, `gw-release.yml` falha fechado.
- Environments e rulesets descritos em `.github/repository-policy.json`, aplicados com
  `groundwork github --sync` (ver [REPOSITORY-POLICY.md](REPOSITORY-POLICY.md)).

## Runbooks
- [Release](../docs/runbooks/release.md)
- Atualização do pipeline: `groundwork update --dry-run`, `groundwork update` e
  `groundwork github` a cada nova versão do Groundwork.
