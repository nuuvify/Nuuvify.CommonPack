# Tutorial de configuração manual no GitHub

Este documento mostra exatamente onde e como executar as configurações manuais no GitHub para ativar o fluxo completo de CI/CD, release e proteção de branches do repositório.

Use este tutorial depois que os workflows já estiverem versionados no repositório.

Para a ordem completa de execução até o desligamento do Azure DevOps, use também [github-cutover-checklist.md](./github-cutover-checklist.md).

No dia da virada operacional, use [github-go-live-checklist.md](./github-go-live-checklist.md).

## Pré-requisitos

- Ser administrador do repositório no GitHub
- Ter em mãos as chaves:
  - `NUGET_API_KEY`
  - `NUGETTEST_API_KEY`
- Confirmar que as branches `main`, `qas` e `nugettest/qas` já existem no remoto

## 1. Abrir as configurações do repositório

No GitHub, entre no repositório e acesse:

1. `Settings`
2. `General`
3. `Security`
4. `Branches`
5. `Environments`
6. `Actions`

## 2. Habilitar features básicas

Caminho: `Settings > General > Features`

Confirme:

- `Issues`: habilitado
- `Discussions`: habilitado
- `Projects`: opcional
- `Wiki`: opcional

Ainda em `Settings > General`, recomendo:

- `Automatically delete head branches`: habilitado
- `Allow squash merging`: habilitado
- `Allow rebase merging`: opcional
- `Allow merge commits`: opcional, conforme sua estratégia

## 3. Validar CODEOWNERS

Caminho: `Settings > General`

Depois de fazer push do arquivo `.github/CODEOWNERS`, abra um PR de teste para `main` e confirme se o GitHub solicita revisão de `@lzocateli` automaticamente.

Sem isso, a regra `Require review from Code Owners` não terá o efeito esperado.

## 4. Configurar branch protection da `main`

Caminho: `Settings > Branches > Add branch protection rule`

Branch name pattern:

```text
main
```

Habilite:

- `Require a pull request before merging`
- `Require approvals`: `1`
- `Dismiss stale pull request approvals when new commits are pushed`
- `Require review from Code Owners`
- `Require conversation resolution before merging`
- `Require status checks to pass before merging`
- `Require branches to be up to date before merging`
- `Do not allow bypassing the above settings`, se quiser endurecer o fluxo
- `Restrict pushes that create files larger than 100 MB`, opcional
- `Do not allow force pushes`
- `Do not allow deletions`

Resultado esperado:

- nenhum PR para `main` será mergeado sem sua aprovação
- os checks obrigatórios terão de passar antes do merge

## 5. Configurar branch protection da `qas`

Caminho: `Settings > Branches > Add branch protection rule`

Branch name pattern:

```text
qas
```

Habilite:

- `Require a pull request before merging`
- `Require status checks to pass before merging`
- `Require branches to be up to date before merging`
- `Do not allow force pushes`
- `Do not allow deletions`

## 6. Configurar branch protection da `nugettest/qas`

Caminho: `Settings > Branches > Add branch protection rule`

Branch name pattern:

```text
nugettest/qas
```

Habilite:

- `Require a pull request before merging`
- `Require status checks to pass before merging`
- `Do not allow force pushes`
- `Do not allow deletions`

## 7. Definir os required checks

Caminho: `Settings > Branches > Edit rule`

Depois que os workflows rodarem ao menos uma vez, marque como obrigatórios:

Para `main`:

- `PR Validation / Build and unit tests`
- `PR Validation / Integration tests`
- `Community Validation / Validate community assets`
- `Workflow Validation / Lint workflows`

Para `qas`:

- `PR Validation / Build and unit tests`
- `PR Validation / Integration tests`
- `Community Validation / Validate community assets`
- `Workflow Validation / Lint workflows`

Para `nugettest/qas`:

- `PR Validation / Build and unit tests`
- `Community Validation / Validate community assets`
- `Workflow Validation / Lint workflows`

Se quiser, também pode exigir `Integration tests` em `nugettest/qas`, mas o fluxo atual foi desenhado para tornar isso opcional.

## 8. Criar os environments

Caminho: `Settings > Environments`

Crie:

- `production`
- `preview`
- `nugettest`

### `production`

Uso esperado: publishes da branch `main`.

Opcionalmente habilite `Required reviewers` se quiser uma segunda barreira manual antes do publish estável.

### `preview`

Uso esperado: publishes da branch `qas`.

Normalmente não precisa de approval manual.

### `nugettest`

Uso esperado: publishes da branch `nugettest/qas`.

Normalmente não precisa de approval manual.

## 9. Configurar os secrets

Caminho: `Settings > Secrets and variables > Actions`

Crie os repository secrets:

- `NUGET_API_KEY`
- `NUGETTEST_API_KEY`

Se preferir, você pode mover esses segredos para os environments em vez de deixá-los no escopo do repositório.

Recomendação:

- `NUGET_API_KEY` no environment `production` e `preview`
- `NUGETTEST_API_KEY` no environment `nugettest`

## 10. Permissões do GitHub Actions

Caminho: `Settings > Actions > General`

Confirme:

- Actions habilitadas para o repositório
- Permissão para rodar ações do marketplace GitHub
- `Workflow permissions`: `Read and write permissions`

Essa última é importante para permitir criação de tags e releases pelo workflow de publish.

Se quiser ser mais restritivo, mantenha permissões mínimas por workflow, como já foi configurado nos arquivos YAML, mas o repositório precisa permitir escrita para criação de release/tag.

## 11. Sincronizar labels e validar triagem

Depois do push dos arquivos abaixo:

- `.github/labels.yml`
- `.github/labeler.yml`
- `.github/release.yml`
- `.github/workflows/label-sync.yml`
- `.github/workflows/pr-triage.yml`

faça o seguinte:

1. Abra a aba `Actions`.
2. Execute manualmente o workflow `Label Sync`, se necessário.
3. Confirme se os labels foram criados no caminho `Issues > Labels`.
4. Abra um PR de teste mexendo em `docs/` ou `test/`.
5. Verifique se o workflow `PR Triage` aplicou labels automaticamente por caminho.

Resultado esperado:

- o repositório passa a ter catálogo padronizado de labels
- PRs recebem labels iniciais automaticamente
- as release notes passam a ser categorizadas a partir desses labels

## 12. Teste controlado do fluxo

Faça o teste nesta ordem:

1. Abra um PR para `main` sem aprová-lo.
2. Verifique se o merge está bloqueado.
3. Aprove o PR com `@lzocateli`.
4. Faça merge e confirme que:
   - os workflows executaram
   - houve publish no NuGet.org
   - uma tag `vX.Y.Z` foi criada
   - uma GitHub Release foi criada

Depois repita:

1. Um PR para `qas` e confirme publish preview.
2. Um PR para `nugettest/qas` e confirme publish `dev` em `https://int.nugettest.org/`.

## 13. Desativar o Azure DevOps

Só faça isso depois de validar os três fluxos:

- `main` estável
- `qas` preview
- `nugettest/qas` dev

Checklist:

1. Remover badges antigos do Azure DevOps dos READMEs.
2. Atualizar documentação interna para GitHub Actions como fonte oficial.
3. Desabilitar triggers e pipelines ativos no Azure DevOps.
4. Garantir que não exista publicação duplicada para os mesmos pacotes.

Sugestão prática:

1. Primeiro valide `nugettest/qas`.
2. Depois valide `qas`.
3. Por último valide `main`.
4. Só então desligue o Azure DevOps.

## 14. Solução de problemas rápida

### O PR para `main` não exige sua aprovação

Verifique:

- se `.github/CODEOWNERS` foi reconhecido
- se a rule de `main` tem `Require review from Code Owners`
- se o PR realmente aponta para `main`

### O workflow não consegue criar tag ou release

Verifique:

- `Settings > Actions > General > Workflow permissions`
- permissões `contents: write` no workflow
- se a branch protection não bloqueia o comportamento esperado do bot

### O publish falha no NuGet

Verifique:

- se `NUGET_API_KEY` está correto
- se a versão não foi publicada anteriormente
- se os pacotes foram realmente gerados em `./artifacts`

### Os labels não foram criados automaticamente

Verifique:

- se o workflow `Label Sync` executou com sucesso
- se o repositório permite actions do marketplace
- se `.github/labels.yml` está válido e no branch correto

### O PR não recebeu labels automáticos

Verifique:

- se o workflow `PR Triage` foi disparado
- se `.github/labeler.yml` cobre os caminhos alterados
- se o token do workflow tem permissão `pull-requests: write`

### O publish falha no nugettest

Verifique:

- se `NUGETTEST_API_KEY` está correto
- se o feed `https://int.nugettest.org/` aceita push com a chave atual

## Resumo do que voce precisa executar manualmente

- configurar branch protections
- configurar required checks
- criar environments
- cadastrar secrets
- executar `Label Sync`
- validar PRs reais em `main`, `qas` e `nugettest/qas`
- desligar os pipelines e triggers do Azure DevOps
