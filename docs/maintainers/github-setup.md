# Configuração do GitHub

Este guia descreve o passo a passo recomendado para deixar o repositório pronto para contribuições externas com governança mínima e boa proteção operacional.

Para um tutorial operacional detalhado com caminhos da interface do GitHub, use também [github-manual-setup-tutorial.md](./github-manual-setup-tutorial.md).

## 1. Configurações gerais do repositório

No GitHub, abra `Settings` do repositório e revise:

- `General > Features`
- `General > Pull Requests`
- `Security`
- `Branches`
- `Code security and analysis`, se disponível

Recomendações:

- habilitar `Discussions`
- habilitar `Issues`
- habilitar `Projects` apenas se fizer parte do fluxo da equipe
- habilitar `Automatically delete head branches`
- permitir `Squash merge` e `Rebase merge` conforme a estratégia desejada

## 2. GitHub Discussions

Em `Settings > General > Features`:

1. Ative `Discussions`.
2. Crie categorias mínimas, por exemplo:
   - `Q&A`
   - `Ideas`
   - `Show and tell`
3. Fixe um post inicial explicando quando usar Discussions e quando usar Issues.

## 3. Segurança

Em `Security`:

1. Ative `Private vulnerability reporting`.
2. Revise se o arquivo `.github/SECURITY.md` está publicado e correto.
3. Se usar Dependabot, mantenha `.github/dependabot.yml` ativo e atualizado.

## 4. CODEOWNERS

Confirme que o GitHub reconheceu `.github/CODEOWNERS`.

Com a configuração atual, `@lzocateli` será solicitado por padrão para revisão das áreas mapeadas.

## 5. Políticas do repositório (rulesets, merge e Environments)

As regras da `main`, das tags `v*`, das opções de merge e dos Environments são versionadas em
[.github/repository-policy.json](../../.github/repository-policy.json), descritas em
[.github/REPOSITORY-POLICY.md](../../.github/REPOSITORY-POLICY.md) e aplicadas pelo
`groundwork github` do Altivy.Groundwork. Não configure branch protection clássica nem rulesets à
mão: a configuração manual diverge do contrato e é removida ou reportada na próxima reconciliação.

```bash
# Conferir divergências (somente leitura; código 0 = conforme)
groundwork github
# Aplicar o contrato
groundwork github --sync
# Remover a proteção clássica e os rulesets legados depois de validar o CI
groundwork github --sync --replace-legacy
```

Resumo do contrato:

- `gw-main-integrity`: sem deleção, sem force push, histórico linear e check `Result`
  obrigatório, **sem bypass**.
- `gw-main-review`: PR obrigatório com 1 aprovação e somente squash; admins podem fazer merge do
  próprio PR via bypass em modo `pull_request`, mas nunca com o `Result` vermelho.
- `gw-release-tags`: somente o GitHub App de release cria, altera ou apaga tags `v*`.
- Environments `release` (revisor `@lzocateli`; `main` e `tag:v*`) e `preview` (`main`).

## 6. Check obrigatório

O único check obrigatório da `main` é o `Result` do workflow [CI](../../.github/workflows/ci.yml),
que agrega `Hygiene`, `Gitleaks`, `.NET` (build, testes `Category=Unit` e pack no container da
toolchain), `Integration tests`, `SonarCloud` e `Package changelog`. Um job pulado ou cancelado
reprova o `Result`.

`Validate community assets` continua com filtro de caminho e **não** deve ser obrigatório.

## 6.1 Secrets e variáveis

| Nome | Tipo | Onde | Uso |
| --- | --- | --- | --- |
| `RELEASE_APP_ID` | Variável | Organização `nuuvify` (repositórios selecionados) | GitHub App de release |
| `RELEASE_APP_PRIVATE_KEY` | Secret | Organização `nuuvify` (repositórios selecionados) | GitHub App de release |
| `NUGET_API_KEY` | Secret | Environment `release` | Push no NuGet.org |
| `SONAR_TOKEN` | Secret | Organização `nuuvify` | Análise do SonarCloud |

Para criar o `NUGET_API_KEY` no Environment `release` sem expor o valor no histórico do shell:

```bash
gh secret set NUGET_API_KEY --repo nuuvify/Nuuvify.CommonPack --env release
```

O processo de release está em [release-process.md](./release-process.md).

## 6.2 Variável de runner do GitHub Actions

Para padronizar o SO de execução dos workflows sem editar múltiplos arquivos, configure uma variável de repositório:

- `GH_ACTIONS_UBUNTU_RUNNER` = `ubuntu-24.04`

Onde configurar:

- `Settings > Secrets and variables > Actions > Variables`

Opcao recomendada para padronizacao entre repositorios:

- Criar em nivel de organizacao: `Organization settings > Secrets and variables > Actions > Variables`
- Nome: `GH_ACTIONS_UBUNTU_RUNNER`
- Valor: `ubuntu-24.04`
- Repositorios com acesso: incluir `Nuuvify.CommonPack` (ou `All repositories`, se fizer sentido para a organizacao)

Precedencia importante:

- Se existir a mesma variavel no repositorio e na organizacao, a variavel do repositorio prevalece.
- Para evitar ambiguidade, mantenha apenas uma fonte de verdade (organizacao ou repositorio).

Observação operacional:

- Os workflows possuem fallback para `ubuntu-24.04`, mas manter a variável definida facilita upgrades futuros (por exemplo, `ubuntu-26.04`) com uma única alteração.

## 7. Templates e formulários

Confirme o reconhecimento automático de:

- `.github/ISSUE_TEMPLATE/bug_report.yml`
- `.github/ISSUE_TEMPLATE/feature_request.yml`
- `.github/ISSUE_TEMPLATE/config.yml`
- `.github/PULL_REQUEST_TEMPLATE.md`

Para Discussions, confirme também:

- `.github/DISCUSSION_TEMPLATE/q-a.yml`
- `.github/DISCUSSION_TEMPLATE/ideas.yml`
- `.github/DISCUSSION_TEMPLATE/show-and-tell.yml`

Para o post inicial fixado, use como base:

- `docs/maintainers/github-discussions-initial-post.md`

## 8. Merge policy recomendada

Para manter histórico legível:

- use títulos de PR claros
- exija descrição objetiva do impacto técnico
- use títulos em Conventional Commits: eles viram a mensagem do squash e o CHANGELOG raiz
- todo PR vai para `main`; releases estáveis saem somente pelo Release PR do release-please

## 9. Labels recomendadas

Crie ao menos estas labels:

- `bug`
- `enhancement`
- `documentation`
- `good first issue`
- `help wanted`
- `breaking change`
- `needs reproduction`
- `security`

Os labels oficiais e suas descrições estão versionados em `.github/labels.yml`.

Para a operação diária de triagem, consulte também [triage-guide.md](./triage-guide.md).

## 10. Governança de acesso

A organização deve manter exatamente `lzocateli` e `eusener` como owners e administradores do repositório. Colaboradores devem receber `Write` pelo time `collaborators`, sem grants humanos diretos.

Para auditar e reconciliar esse estado, siga [github-access-governance.md](./github-access-governance.md).

## 11. Checklist final

- Discussions habilitado
- Private vulnerability reporting habilitado
- `groundwork github` sem divergências (código 0)
- Sua aprovação obrigatória validada em PR para `main`
- CODEOWNERS reconhecido
- Issue forms ativos
- PR template ativo
- `NUGET_API_KEY` configurado no Environment `release`
- Labels iniciais criadas
- Governança de acesso auditada sem `DRIFT` ou `BLOCKED`

## 12. Automação de labels e triagem

Depois de subir os arquivos de automação:

- `Label Sync` sincroniza os labels versionados em `.github/labels.yml`
- `PR Triage` aplica labels iniciais por caminho em pull requests
