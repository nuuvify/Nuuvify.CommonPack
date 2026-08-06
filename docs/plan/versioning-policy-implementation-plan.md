# Plano de implementação da política de versionamento

## Gestao de Status

| Campo | Valor |
|---|---|
| Status | Concluido |
| Criado em | 2026-08-06 |
| Atualizado em | 2026-08-06 |
| Responsavel | Maintainers |
| Ultima revisao | 2026-08-06 |

### Historico de Status

| Data | De -> Para | Motivo |
|---|---|---|
| 2026-08-06 | Em andamento -> Concluido | Implementacao validada e encerrada |

Adotar `src/Directory.Build.props` como única fonte versionada do próximo release estável, remover Commitizen e impedir incrementos implícitos no deploy. Um workflow manual abrirá o PR de versão; os canais apenas derivarão sufixos SemVer. `AssemblyVersion` ficará estável por major, enquanto `FileVersion`, `InformationalVersion`, pacote, tag e GitHub Release carregarão a versão exata e serão verificados antes da publicação.

## Estado atual

- `src/Directory.Build.props` declara `Version=2.7.0` e `FileVersion=1520`; o SDK deriva `AssemblyVersion=2.7.0.0` e `AssemblyInformationalVersion=2.7.0+<sha>`.
- O build real confirma `AssemblyFileVersion("1520")`, portanto o arquivo binário não identifica a versão NuGet/release.
- `.github/scripts/compute-version.ps1` compara a versão do props com tags estáveis e incrementa patch implicitamente quando encontra uma tag igual. Para `qas` e `nugettest/qas`, acrescenta `preview.<run_number>` ou `dev.<run_number>`.
- `.github/workflows/publish-release.yml` sobrescreve `Version` e `PackageVersion`, publica todos os pacotes e somente depois cria a tag/release. Não valida os metadados internos dos assemblies.
- `.cz.toml` declara `2.7.0-preview.1`, mas nenhum workflow chama Commitizen; hoje é uma segunda fonte de verdade inativa e contradiz a documentação.

## Política decidida

- Bump SemVer explícito por release PR automatizado, sem inferência por commits ou labels.
- `VersionPrefix=X.Y.Z` no props é a única versão base persistida.
- `AssemblyVersion=X.0.0.0`, preservando compatibilidade binária dentro da major.
- Stable: pacote/tag/release `X.Y.Z`, `FileVersion=X.Y.Z.0`, informacional `X.Y.Z+<sha>`.
- Preview: pacote/tag/pre-release `X.Y.Z-preview.<run_number>`, `FileVersion=X.Y.Z.<run_number>`, informacional com versão completa e SHA.
- Dev: pacote `X.Y.Z-dev.<run_number>`, sem GitHub Release formal; `FileVersion=X.Y.Z.<run_number>` e informacional com SHA.
- Nenhum workflow incrementa major/minor/patch durante o deploy.

## Etapas

1. ✅ **Centralizar propriedades MSBuild**: substituir `Version` por `VersionPrefix` em `src/Directory.Build.props`; derivar explicitamente `AssemblyVersion` por major; definir defaults locais para `FileVersion` e habilitar SHA em `InformationalVersion`. Permitir override somente para valores efêmeros de CI (`VersionSuffix`, revisão do arquivo e SHA).
2. ✅ **Remover Commitizen**: excluir `.cz.toml`; remover referências a Commitizen de `docs/maintainers/release-process.md` e `docs/maintainers/changelog-standardization-plan.md`; registrar a mudança no `CHANGELOG.md`.
3. ✅ **Reescrever o cálculo de versão**: alterar `.github/scripts/compute-version.ps1` para ler `VersionPrefix`, validar SemVer estável estrito, recusar versão menor/igual à última tag no primeiro release, não fazer bump implícito e emitir `version`, `version_prefix`, `version_suffix`, `assembly_version`, `file_version`, `informational_version`, `tag`, canal e feed. Reexecução só poderá reutilizar versão quando a tag apontar para o mesmo commit.
4. ✅ **Automatizar o release PR**: criar `.github/workflows/prepare-release.yml`, acionado por `workflow_dispatch`, com inputs `version`, `target_branch` e modo de changelog. O job valida que `X.Y.Z` é maior que a última stable, cria branch `release/vX.Y.Z`, atualiza `VersionPrefix`, fecha a seção `Não Lançado` para stable, restaura uma seção vazia e abre PR via `gh`. Para preview/dev, mantém `Não Lançado` aberto e altera apenas a versão base quando necessário.
5. ✅ **Fortalecer validação de PR**: adicionar a `.github/workflows/pr-validation.yml` um job de política de versão que valide formato, monotonicidade, alteração acompanhada de changelog e proíba editar propriedades derivadas. Em PR para `main`, exigir que a versão seja superior à última tag quando `VersionPrefix` mudar. Incluir auditoria de artefatos com `assert-artifact-version.ps1`.
6. ✅ **Alinhar build e publicação**: refatorar `.github/workflows/publish-release.yml` para passar `VersionPrefix`, `VersionSuffix`, `AssemblyVersion`, `FileVersionRevision`, `ContinuousIntegrationBuild=true` e `RepositoryCommit`. Criar a tag antes do push NuGet; em falha, a reexecução deve reconhecer a mesma tag/commit e continuar idempotentemente. GitHub Release só é publicada depois de todos os pacotes terem sido enviados.
7. ✅ **Validar artefatos reais**: criar `.github/scripts/assert-artifact-version.ps1` para abrir cada `.nupkg`, validar versão do `.nuspec` e inspecionar todos os assemblies em `lib/**` com `AssemblyName`/`FileVersionInfo`: assembly `X.0.0.0`, arquivo esperado e produto/informacional contendo versão SemVer e SHA. Executar após `pack` e antes de upload/publish, tanto em PR quanto no release.
8. ✅ **Ajustar release notes**: atualizar `.github/scripts/build-release-notes.ps1` para stable ler a seção fechada `[X.Y.Z]`; preview/dev usar `Não Lançado`. Aceitar `Unreleased` em transição. Falhar cedo se versão, data ou conteúdo não corresponderem ao release.
9. ✅ **Atualizar documentação operacional**: documentar matriz das cinco versões .NET, fluxo do release PR, promoção `dev -> preview -> stable`, reexecução, rollback e regra de breaking change major em `docs/maintainers/release-process.md`, `github-setup.md` e `changelog-standardization-plan.md`.

## Arquivos relevantes

- `src/Directory.Build.props`: fonte única e propriedades de assembly.
- `.cz.toml`: remover.
- `.github/scripts/compute-version.ps1`: cálculo determinístico sem bump implícito.
- `.github/scripts/assert-artifact-version.ps1`: novo auditor de nupkg/assemblies.
- `.github/scripts/build-release-notes.ps1`: notas da seção correspondente à versão.
- `.github/workflows/prepare-release.yml`: novo release PR manual.
- `.github/workflows/pr-validation.yml`: política de versão e auditoria de pacotes.
- `.github/workflows/publish-release.yml`: build, tag, publish e release alinhados.
- `docs/maintainers/release-process.md`, `docs/maintainers/github-setup.md`, `docs/maintainers/github-manual-setup-tutorial.md`, `docs/maintainers/changelog-standardization-plan.md` e `CHANGELOG.md`: contrato operacional.

## Verificação

1. Testes de tabela do script para primeira versão, patch/minor/major, versão igual ou menor à tag, preview/dev e reexecução no mesmo commit.
2. `dotnet msbuild` para conferir propriedades efetivas localmente e em cada canal.
3. `dotnet build`, testes Unit/Integration e `dotnet pack` com stable, preview e dev simulados.
4. Auditoria de todos os `.nupkg`: nuspec, `AssemblyVersion`, `FileVersion` e `ProductVersion/InformationalVersion`.
5. `actionlint` em todos os workflows e parser PowerShell em todos os scripts.
6. Teste de fumaça em `nugettest/qas`, depois `qas`, por fim release PR em `main`; confirmar pacote, tag, SHA e GitHub Release idênticos.

## Limites

- Mantém versionamento único para todos os pacotes do monorepo; versionamento independente por pacote fica fora deste ciclo.
- Não adiciona dependências de terceiros; usa PowerShell, MSBuild, Git e `gh` já disponíveis nos runners.
- Dev continua sem GitHub Release formal; preview e stable terão tag correspondente ao pacote.
