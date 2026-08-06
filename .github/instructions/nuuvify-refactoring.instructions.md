---
description: "Use when refactoring, simplifying, renaming, cleaning up, or reducing complexity in Nuuvify.CommonPack libraries. Covers behavior preservation, smaller slices, API safety, duplication removal, and validation-first refactoring."
name: "Nuuvify Library Refactoring"
---

# Diretrizes para refatoração

- Refatore em fatias pequenas e validáveis.
- Preserve comportamento observável antes de melhorar estrutura interna.
- Remova duplicação apenas quando o novo ponto comum tiver dono claro e semântica estável.
- Reduza complexidade cognitiva sem esconder fluxo em abstrações artificiais.
- Antes de mover código entre pacotes, valide se a dependência resultante continua coerente com a arquitetura.
- Renomeie símbolos públicos só quando o ganho justificar o custo de compatibilidade.

## Ordem preferida

1. Cobrir ou ajustar testes do comportamento atual.
2. Aplicar a menor extração, renomeação ou simplificação necessária.
3. Revalidar com o teste mais estreito possível.
