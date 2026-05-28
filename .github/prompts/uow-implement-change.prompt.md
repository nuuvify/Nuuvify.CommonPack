---
description: "Implementar mudança em Nuuvify.CommonPack.UnitOfWork ou UnitOfWork.Abstraction preservando composição de queries, contratos públicos, paginação, filtros e ordenação."
name: "UoW Implement Change"
argument-hint: "Objetivo, comportamento esperado, símbolo ou arquivo e validação desejada"
agent: "agent"
model: "GPT-5 (copilot)"
---

Implemente a mudança pedida em `Nuuvify.CommonPack.UnitOfWork` ou `Nuuvify.CommonPack.UnitOfWork.Abstraction`.

Restrições principais:
- preserve composição de consultas e contratos públicos
- não quebre tradução para SQL sem evidência e validação
- trate paginação, filtros e ordenação como API observável
- prefira a menor mudança possível no ponto que realmente controla o comportamento

Checklist de saída:
1. mudança implementada
2. teste ou validação estreita executada
3. risco residual informado, se houver

Use também:
- [UnitOfWork Source Instruction](../instructions/unitofwork-source.instructions.md)
- [Library Tests Instruction](../instructions/library-tests.instructions.md)