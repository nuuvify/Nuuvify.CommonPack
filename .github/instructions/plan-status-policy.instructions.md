---
description: "Use quando criar, mover ou atualizar arquivos de plano no repositorio Nuuvify.CommonPack."
name: "Nuuvify Plan Status Policy"
applyTo: "docs/plan/**/*.md"
---

# Politica de Planos em Arquivo Fisico

## Objetivo
Padronizar onde os planos sao armazenados e como o status de execucao e gerenciado ao longo do ciclo de vida.

## Regra obrigatoria de localizacao
Todo plano gravado fisicamente no repositorio deve estar em `docs/plan/`.

Regras:
- Nao criar novos planos em `docs/ai/`, `docs/maintainers/` ou outras pastas.
- Quando um plano existente estiver fora de `docs/plan/`, ele deve ser movido no mesmo PR da alteracao.

## Regra obrigatoria de gestao de status
Todo arquivo de plano deve conter uma secao `Gestao de Status` no inicio do documento, antes da descricao detalhada, com no minimo:
- `Status`: `Rascunho`, `Em andamento`, `Bloqueado`, `Concluido` ou `Arquivado`
- `Criado em`: data no formato `AAAA-MM-DD`
- `Atualizado em`: data no formato `AAAA-MM-DD`
- `Responsavel`: time, area ou pessoa
- `Ultima revisao`: data no formato `AAAA-MM-DD` ou `N/A`

Tambem deve existir um mini-historico com transicoes de status:
- Data
- De -> Para
- Motivo

## Template minimo
Use este bloco no inicio de qualquer novo plano:

```markdown
## Gestao de Status

| Campo | Valor |
|---|---|
| Status | Rascunho |
| Criado em | 2026-08-06 |
| Atualizado em | 2026-08-06 |
| Responsavel | Definir |
| Ultima revisao | N/A |

### Historico de Status

| Data | De -> Para | Motivo |
|---|---|---|
| 2026-08-06 | N/A -> Rascunho | Criacao do plano |
```
