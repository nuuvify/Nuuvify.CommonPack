# Checklist final de cutover para GitHub

Este documento consolida a ordem recomendada para concluir a migração operacional do repositório para GitHub como plataforma única de colaboração, CI/CD e release.

Use este checklist quando os workflows e documentos já estiverem publicados no repositório.

## Objetivo do cutover

Ao final deste checklist, o estado desejado é:

- GitHub Actions como pipeline oficial
- releases estáveis saindo de `main`
- previews saindo de `qas`
- builds `dev` saindo de `nugettest/qas`
- PRs para `main` exigindo sua aprovação como code owner
- Azure DevOps desativado sem risco de publicação duplicada

## Ordem recomendada

## 1. Preparar governança e protections

- [ ] Confirmar que `.github/CODEOWNERS` está reconhecido pelo GitHub
- [ ] Configurar branch protection em `main`
- [ ] Configurar branch protection em `qas`
- [ ] Configurar branch protection em `nugettest/qas`
- [ ] Validar que PR para `main` exige aprovação de `@lzocateli`

## 2. Preparar Actions, environments e secrets

- [ ] Habilitar GitHub Actions no repositório
- [ ] Configurar `Workflow permissions` com escrita
- [ ] Criar environments `production`, `preview` e `nugettest`
- [ ] Configurar `NUGET_API_KEY`
- [ ] Configurar `NUGETTEST_API_KEY`

## 3. Ativar checks obrigatórios

- [ ] Rodar o workflow `PR Validation` ao menos uma vez
- [ ] Rodar `Community Validation` ao menos uma vez
- [ ] Rodar `Workflow Validation` ao menos uma vez
- [ ] Marcar os checks obrigatórios nas branch protections

## 4. Ativar labels, triagem e release categories

- [ ] Rodar `Label Sync`
- [ ] Confirmar labels em `Issues > Labels`
- [ ] Abrir um PR de teste e validar `PR Triage`
- [ ] Confirmar que `.github/release.yml` está sendo usado nas release notes

## 5. Validar fluxos por branch

### `nugettest/qas`

- [ ] Abrir PR de teste para `nugettest/qas`
- [ ] Confirmar merge com checks verdes
- [ ] Confirmar publish `dev` em `https://int.nugettest.org/`

### `qas`

- [ ] Abrir PR de teste para `qas`
- [ ] Confirmar publish preview no NuGet.org
- [ ] Confirmar criação de pre-release no GitHub, se aplicável

### `main`

- [ ] Abrir PR de teste para `main`
- [ ] Confirmar bloqueio sem sua aprovação
- [ ] Aprovar com `@lzocateli`
- [ ] Confirmar publish estável no NuGet.org
- [ ] Confirmar tag `vX.Y.Z`
- [ ] Confirmar GitHub Release criada corretamente

## 6. Validar documentação final

- [ ] Confirmar que o README principal aponta para GitHub Actions
- [ ] Confirmar que os READMEs dos pacotes não apontam mais para Azure DevOps
- [ ] Confirmar que o tutorial manual está atualizado
- [ ] Confirmar que o processo de release reflete GitHub-only

## 7. Desligar Azure DevOps com segurança

- [ ] Confirmar que `stable`, `preview` e `dev` já foram testados com sucesso no GitHub
- [ ] Desabilitar triggers do pipeline no Azure DevOps
- [ ] Desabilitar publicação de pacotes no Azure DevOps
- [ ] Remover badges ou documentação residual que ainda trate Azure DevOps como operacional
- [ ] Comunicar internamente que o GitHub é a plataforma oficial de CI/CD e release

## 8. Pós-cutover

- [ ] Monitorar os primeiros PRs e releases no GitHub
- [ ] Ajustar labels e required checks se houver ruído operacional
- [ ] Revisar se vale exigir approval manual do environment `production`
- [ ] Revisar se vale publicar apenas pacotes alterados em uma fase futura

## O que ainda é manual

As etapas abaixo não são feitas por arquivo versionado e exigem ação manual sua no GitHub ou fora dele:

- criação de environments
- cadastro de secrets
- branch protection rules
- required checks
- execução inicial do `Label Sync`
- validação prática dos merges por branch
- desligamento do Azure DevOps

## Referências

- [github-manual-setup-tutorial.md](./github-manual-setup-tutorial.md)
- [github-go-live-checklist.md](./github-go-live-checklist.md)
- [github-setup.md](./github-setup.md)
- [release-process.md](./release-process.md)
- [triage-guide.md](./triage-guide.md)
