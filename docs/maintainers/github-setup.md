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

Quando os checks estiverem aparecendo nos PRs, marque como obrigatórios:

**Sempre executam em PRs para `main`, `qas` e `nugettest/qas`:**

- `Version policy check`
- `Verify package CHANGELOG updated`

**Path-filtered — executam apenas quando os paths relevantes são alterados (não adicionar como required):**

- `Lint workflows` — somente quando `.github/workflows/**` muda
- `Validate community assets` — somente quando `.github/**`, `docs/**`, `Readme.md` ou `CHANGELOG.md` mudam

Configuração recomendada em `Require status checks to pass before merging`:

- Required: `Version policy check`
- Required: `Verify package CHANGELOG updated`
- Não required: `Lint workflows`
- Não required: `Validate community assets`

Motivo: checks path-filtered podem não executar em todos os PRs; se forem marcados como required, o merge pode ficar bloqueado sem necessidade.

> `Build and unit tests` e `Integration tests` também rodam em PRs (workflow `PR Validation`) e podem rodar novamente no fluxo de publicação.

## 6.1 Environments e segredos

Onde configurar no GitHub:

- `Settings > Environments`

Crie os environments (botao `New environment`):

- `production`
- `preview`
- `nugettest`

Mapeamento usado pelo workflow `publish-release.yml`:

- branch `main` -> environment `production`
- branch `qas` -> environment `preview`
- branch `nugettest/qas` -> environment `nugettest`

Configure os secrets em cada environment:

- Em `production`:
   - `NUGET_API_KEY` (token do NuGet.org)
- Em `preview`:
   - `NUGET_API_KEY` (token do NuGet.org)
- Em `nugettest`:
   - `NUGETTEST_API_KEY` (token do `https://int.nugettest.org/`)

Como adicionar secret (por environment):

1. `Settings > Environments > <environment>`
2. Seção `Environment secrets`
3. `Add secret`
4. `Name`: use exatamente o nome esperado pelo workflow
5. `Value`: cole o token
6. `Add secret` para salvar

Opcional de governanca:

- Em `production`, configure `Required reviewers` para exigir aprovação antes do job de publish.
- Se quiser, aplique a mesma proteção em `preview`.

Validacao rapida apos configurar:

1. Rode manualmente `publish-release.yml` via `Actions > Publish and Release > Run workflow` apontando para cada branch.
2. Confirme no job `Publish packages and release` que o campo `Environment` corresponde ao branch.
3. Se secret faltar, o job falha com mensagem explicita indicando o nome do secret ausente.

O workflow `Publish and Release` é disparado por `push` em `main`, `qas` e `nugettest/qas`. As branch protections devem impedir push direto para que somente commits aprovados em PR sejam publicados.

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
