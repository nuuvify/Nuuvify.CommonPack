# Guia de triagem

Este documento define como issues e pull requests devem ser classificados, priorizados e preparados para revisão e release.

## Objetivos

- padronizar labels
- acelerar roteamento inicial
- melhorar qualidade das release notes
- facilitar contribuição externa

## Labels oficiais

Os labels são mantidos em `.github/labels.yml`.

Grupos principais:

- `bug`: erro reproduzível
- `enhancement`: melhoria incremental
- `feature`: nova funcionalidade
- `documentation` e `docs`: mudanças documentais
- `test`: mudanças em testes
- `quality`: manutenção interna e melhorias de engenharia
- `refactor`: refatoração sem mudança funcional esperada
- `dependencies`: atualização de dependências
- `security`: segurança
- `breaking change`: mudança incompatível
- `major`: mudança de alto impacto
- `good first issue`: boa issue para onboarding
- `help wanted`: aberta para comunidade
- `needs reproduction`: faltam passos claros de reprodução
- `skip-release-notes`: não deve aparecer nas notas de release

## Fluxo de triagem de issues

### Bug report

1. Confirmar se há passos de reprodução.
2. Se faltar contexto, aplicar `needs reproduction`.
3. Se confirmado, aplicar `bug`.
4. Se for simples e segura para contribuição externa, aplicar `good first issue` ou `help wanted`.

### Feature request

1. Confirmar alinhamento com o escopo do projeto.
2. Aplicar `feature` quando houver capacidade nova.
3. Aplicar `enhancement` quando for evolução incremental.
4. Aplicar `breaking change` se houver impacto incompatível.

### Documentação

1. Aplicar `documentation`.
2. Se for ajuste muito localizado, `docs` também pode ser usado.
3. Considere `skip-release-notes` quando não houver impacto relevante ao usuário final.

## Fluxo de triagem de pull requests

### Labels automáticos

O workflow `PR Triage` aplica labels por caminho usando `.github/labeler.yml`.

Exemplos:

- mudanças em `docs/` tendem a receber `documentation`
- mudanças em `test/` tendem a receber `test`
- mudanças em workflows/scripts tendem a receber `quality`

### Labels manuais complementares

Adicione manualmente quando necessário:

- `bug`
- `feature`
- `enhancement`
- `breaking change`
- `skip-release-notes`

## Relação com as release notes

As categorias automáticas do GitHub Release usam `.github/release.yml`.

Isso significa que os labels aplicados aos PRs impactam diretamente a organização das notas de release.

Recomendação:

- use `feature`, `enhancement`, `bug`, `documentation`, `dependencies` e `breaking change` com consistência
- aplique `skip-release-notes` em mudanças internas sem valor de comunicação externa

## Critérios de priorização sugeridos

- `security` e `breaking change`: prioridade alta
- `bug`: prioridade alta ou média conforme impacto
- `dependencies`: prioridade conforme risco e exposição
- `feature` e `enhancement`: prioridade conforme roadmap
- `documentation`: prioridade média, mas pode ser alta quando bloqueia onboarding

## Checklist rápido do mantenedor

- a issue/PR tem label principal?
- há necessidade de `needs reproduction`?
- ela deve entrar nas release notes?
- ela é adequada para comunidade (`good first issue` ou `help wanted`)?
- existe risco de compatibilidade (`breaking change`)?
