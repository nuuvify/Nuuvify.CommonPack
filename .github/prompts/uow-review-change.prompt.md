---
description: "Revisar mudanças em Nuuvify.CommonPack.UnitOfWork ou UnitOfWork.Abstraction com foco em LINQ/EF translation, paginação, filtros, ordenação, performance e API pública."
name: "UoW Review Change"
argument-hint: "PR, diff, arquivo ou símbolo para revisar"
agent: "library-reviewer"
model: ["Claude Sonnet 4.5 (copilot)", "GPT-5 (copilot)"]
---

Faça uma revisão técnica da mudança em `Nuuvify.CommonPack.UnitOfWork` ou `Nuuvify.CommonPack.UnitOfWork.Abstraction`.

Priorize:
1. regressões em tradução LINQ/EF e composição de query
2. quebra de paginação, filtros, includes e ordenação
3. impacto em contratos públicos e SemVer
4. lacunas de teste e regressão
5. riscos de performance ou compatibilidade entre providers

Use também:
- [UnitOfWork Source Instruction](../instructions/unitofwork-source.instructions.md)
- [Library Review Instruction](../instructions/library-review.instructions.md)