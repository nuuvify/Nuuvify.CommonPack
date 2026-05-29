# Configuração do GitHub

Este guia descreve o passo a passo recomendado para deixar o repositório pronto para contribuições externas com governança mínima e boa proteção operacional.

Para um tutorial operacional detalhado com caminhos da interface do GitHub, use também [github-manual-setup-tutorial.md](./github-manual-setup-tutorial.md).

Para a sequência completa de ativação, validação e desligamento do Azure DevOps, use [github-cutover-checklist.md](./github-cutover-checklist.md).

Para o dia do cutover, use [github-go-live-checklist.md](./github-go-live-checklist.md).

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

## 5. Proteção de branches

Em `Settings > Branches`, configure regras para `main`, `qas` e `nugettest/qas`.

### `main`

Recomendado:

- Require a pull request before merging
- Require approvals: `1`
- Dismiss stale pull request approvals when new commits are pushed
- Require review from Code Owners
- Require conversation resolution before merging
- Require branches to be up to date before merging
- Do not allow force pushes
- Do not allow deletions

Com `CODEOWNERS` apontando para `@lzocateli`, todo PR em `main` exigirá sua aprovação.

### `qas`

Recomendado:

- Require a pull request before merging
- Require branches to be up to date before merging
- Require status checks to pass
- Do not allow force pushes
- Do not allow deletions

### `nugettest/qas`

Recomendado:

- Require a pull request before merging
- Require status checks to pass
- Do not allow force pushes
- Do not allow deletions

## 6. Required status checks

O GitHub Actions deve ser a fonte oficial dos checks obrigatórios.

Quando os checks estiverem aparecendo nos PRs, marque como obrigatórios ao menos:

- `PR Validation / Build and unit tests`
- `PR Validation / Integration tests` para `main` e `qas`
- `Community Validation / Validate community assets`
- `Workflow Validation / Lint workflows`

Para `nugettest/qas`, você pode dispensar integração obrigatória se quiser reduzir custo e tempo de feedback.

## 6.1 Environments e segredos

Crie os environments:

- `production`
- `preview`
- `nugettest`

Configure os secrets:

- `NUGET_API_KEY` para NuGet.org
- `NUGETTEST_API_KEY` para `https://int.nugettest.org/`

Se quiser endurecer a produção, adicione regra de approval no environment `production`.

## 6.2 Variável de runner do GitHub Actions

Para padronizar o SO de execução dos workflows sem editar múltiplos arquivos, configure uma variável de repositório:

- `GH_ACTIONS_UBUNTU_RUNNER` = `ubuntu-24.04`

Onde configurar:

- `Settings > Secrets and variables > Actions > Variables`

Observação operacional:

- Os workflows possuem fallback para `ubuntu-24.04`, mas manter a variável definida facilita upgrades futuros (por exemplo, `ubuntu-26.04`) com uma única alteração.

## 7. Templates e formulários

Confirme o reconhecimento automático de:

- `.github/ISSUE_TEMPLATE/bug_report.yml`
- `.github/ISSUE_TEMPLATE/feature_request.yml`
- `.github/ISSUE_TEMPLATE/config.yml`
- `.github/PULL_REQUEST_TEMPLATE.md`

## 8. Merge policy recomendada

Para manter histórico legível:

- use títulos de PR claros
- exija descrição objetiva do impacto técnico
- prefira squash merge quando o branch contiver commits de trabalho intermediários
- bloqueie merge direto em `main`
- use `main` apenas para releases estáveis
- use `qas` para preview
- use `nugettest/qas` para publicações `dev`

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

## 10. Checklist final

- Discussions habilitado
- Private vulnerability reporting habilitado
- Branch protection configurada
- Sua aprovação obrigatória validada em PR para `main`
- CODEOWNERS reconhecido
- Issue forms ativos
- PR template ativo
- Checks obrigatórios definidos
- Environments e secrets configurados
- Labels iniciais criadas

## 11. Automação de labels e triagem

Depois de subir os arquivos de automação:

- `Label Sync` sincroniza os labels versionados em `.github/labels.yml`
- `PR Triage` aplica labels iniciais por caminho em pull requests
- `.github/release.yml` usa os labels para categorizar as release notes
