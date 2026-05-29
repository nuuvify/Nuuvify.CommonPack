# Processo de release

Este documento descreve o fluxo recomendado para preparar e publicar uma nova versão do projeto.

## Base atual

- O projeto usa SemVer 2.0.
- O versionamento é controlado por Commitizen em `.cz.toml`.
- A versão principal dos pacotes é definida em `src/Directory.Build.props`.
- O CI/CD oficial está em GitHub Actions.
- O versionamento de publicação é calculado por branch no workflow `publish-release.yml`.
- O runner Linux dos workflows é parametrizado pela variável de repositório `GH_ACTIONS_UBUNTU_RUNNER`, com fallback para `ubuntu-24.04`.

## Antes do release

Confirme:

- Build da solução sem falhas
- Testes relevantes passando
- `CHANGELOG.md` atualizado
- Documentação atualizada quando necessário
- Compatibilidade analisada para mudanças públicas

## Comandos úteis

```powershell
dotnet restore
dotnet build Nuuvify.CommonPack.sln -c Release
dotnet test
```

## Versionamento

O arquivo `.cz.toml` define:

- estratégia `cz_conventional_commits`
- `version_scheme = semver2`
- atualização da versão em `src/Directory.Build.props`
- mensagem padrão de bump com `chore(release)`

## Fluxo por branch

### `main`

- Todo PR exige aprovação de `@lzocateli` via `CODEOWNERS` e branch protection.
- Ao mergear em `main`, o workflow de release:
	- calcula a próxima versão estável
	- compila e testa a solução
	- gera os pacotes
	- publica no NuGet.org
	- cria tag `vX.Y.Z`
	- cria GitHub Release com notas derivadas do `CHANGELOG.md`

### `qas`

- Ao mergear em `qas`, o workflow calcula uma versão `preview`.
- O pacote preview é publicado no NuGet.org.
- Um GitHub pre-release é criado para rastreabilidade usando notas consistentes do changelog.

### `nugettest/qas`

- Ao mergear em `nugettest/qas`, o workflow calcula uma versão `dev`.
- O pacote `dev` é publicado no feed `https://int.nugettest.org/`.
- Esse fluxo nao cria GitHub Release formal.

## Publicação

Os segredos mínimos esperados são:

- `NUGET_API_KEY`
- `NUGETTEST_API_KEY`

Os environments recomendados são:

- `production`
- `preview`
- `nugettest`

## Runner do GitHub Actions (fixo por versão)

Para evitar alterações manuais em vários arquivos de workflow, todos os `runs-on` usam a mesma variável de repositório:

- `GH_ACTIONS_UBUNTU_RUNNER` (exemplo: `ubuntu-24.04`)

Comportamento:

- Se a variável estiver definida, os workflows usam o valor dela.
- Se a variável não estiver definida, o fallback é `ubuntu-24.04`.

Ao trocar de versão, atualize apenas essa variável em `Settings > Secrets and variables > Actions > Variables`.

## Checklist de mantenedor

- Versão correta em `src/Directory.Build.props`
- CHANGELOG coerente com a release
- Sem breaking change não documentado
- Workflows do GitHub Actions verdes
- Tags e notas de release alinhadas

## Cutover operacional

Enquanto o Azure DevOps ainda existir no ambiente, trate o GitHub como fonte de verdade e siga o checklist em [github-cutover-checklist.md](./github-cutover-checklist.md) antes de desligar o pipeline antigo.
