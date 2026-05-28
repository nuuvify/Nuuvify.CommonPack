---
description: "Planejar mudança em Nuuvify.CommonPack.UnitOfWork ou UnitOfWork.Abstraction com foco em IQueryable, filtros dinâmicos, paginação, ordenação, tradução LINQ/EF e compatibilidade pública."
name: "UoW Plan Change"
argument-hint: "Objetivo, símbolo ou arquivo, comportamento esperado, provider ou risco conhecido"
agent: "plan"
model: "GPT-5 (copilot)"
---

Crie um plano enxuto para a mudança solicitada em `Nuuvify.CommonPack.UnitOfWork` ou `Nuuvify.CommonPack.UnitOfWork.Abstraction`.

Foque em:
- impacto em `IQueryable`
- tradução LINQ/EF Core
- paginação, filtros dinâmicos e ordenação
- compatibilidade entre providers
- risco de quebra em extensões e contratos públicos

Entregue:
1. ponto exato do pacote onde o comportamento é decidido
2. hipótese local de implementação
3. menor conjunto de arquivos a alterar
4. testes e validações mais estreitos
5. riscos de regressão funcional, de tradução ou de SemVer

Referências:
- [UnitOfWork README](../../src/Nuuvify.CommonPack.UnitOfWork/README.md)
- [UnitOfWork Source Instruction](../instructions/unitofwork-source.instructions.md)
- [Architecture](../../docs/maintainers/architecture.md)