# Checklist de go live no GitHub

Use este documento no dia em que for concluir o cutover operacional do repositório para GitHub.

Este checklist é propositalmente curto e orientado à execução.

## Antes da janela de cutover

- [ ] Confirmar acesso administrativo ao repositório GitHub
- [ ] Confirmar acesso administrativo ao Azure DevOps para desligamento final
- [ ] Confirmar que `NUGET_API_KEY` está válido
- [ ] Confirmar que `NUGETTEST_API_KEY` está válido
- [ ] Confirmar existência das branches `main`, `qas` e `nugettest/qas`
- [ ] Confirmar que os environments `production`, `preview` e `nugettest` existem
- [ ] Confirmar que os workflows estão no branch padrão mais recente

## Verificações obrigatórias no GitHub

- [ ] `CODEOWNERS` reconhecido
- [ ] Branch protection de `main` ativa com `Require review from Code Owners`
- [ ] Branch protection de `qas` ativa
- [ ] Branch protection de `nugettest/qas` ativa
- [ ] Required checks configurados nas três branches
- [ ] `Workflow permissions` com escrita habilitada
- [ ] `Label Sync` já executado com sucesso
- [ ] `PR Triage` já testado com sucesso

## Testes de fumaça antes do desligamento do Azure DevOps

- [ ] PR de teste para `nugettest/qas` validado
- [ ] Publish `dev` no feed `https://int.nugettest.org/` validado
- [ ] PR de teste para `qas` validado
- [ ] Publish preview no NuGet.org validado
- [ ] PR de teste para `main` validado
- [ ] Bloqueio sem aprovação de `@lzocateli` validado
- [ ] Publish estável no NuGet.org validado
- [ ] Tag `vX.Y.Z` validada
- [ ] GitHub Release validada

## Momento do cutover

- [ ] Congelar merges por alguns minutos, se necessário
- [ ] Confirmar que não há pipelines Azure DevOps em execução
- [ ] Desabilitar triggers do Azure DevOps
- [ ] Desabilitar publish de pacotes no Azure DevOps
- [ ] Confirmar que o GitHub é a única origem ativa de publicação

## Após o cutover

- [ ] Abrir um PR real pequeno e confirmar o fluxo completo
- [ ] Monitorar a aba `Actions` do GitHub nas primeiras execuções
- [ ] Confirmar que não houve publicação duplicada
- [ ] Comunicar a equipe sobre o encerramento do Azure DevOps
- [ ] Registrar internamente a data efetiva do cutover

## Rollback

Se houver falha crítica no publish ou no bloqueio de revisão:

- [ ] interromper merges em `main`
- [ ] corrigir secrets, environments ou branch protection
- [ ] rerodar a validação em `nugettest/qas`
- [ ] só reativar o fluxo estável depois de validar `dev` e `preview`

## Referências rápidas

- [github-manual-setup-tutorial.md](./github-manual-setup-tutorial.md)
- [github-cutover-checklist.md](./github-cutover-checklist.md)
- [github-setup.md](./github-setup.md)
- [release-process.md](./release-process.md)
