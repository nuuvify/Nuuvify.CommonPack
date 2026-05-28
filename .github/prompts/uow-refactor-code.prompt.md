---
description: "Refatorar Nuuvify.CommonPack.UnitOfWork ou UnitOfWork.Abstraction com segurança para IQueryable, LINQ/EF, extensões públicas, paginação, filtros e ordenação."
name: "UoW Refactor Code"
argument-hint: "Símbolo, arquivo, cheiro de código e teste alvo"
agent: "library-refactorer"
model: "GPT-5 (copilot)"
---

Refatore o trecho solicitado em `Nuuvify.CommonPack.UnitOfWork` ou `Nuuvify.CommonPack.UnitOfWork.Abstraction`.

Cuidados obrigatórios:
- preserve comportamento observável em queries encadeadas
- não quebre extensões públicas nem compatibilidade de provider
- revalide com o teste ou cenário mais estreito possível
- evite abstrações que escondam a query final ou dificultem troubleshooting

Use também:
- [UnitOfWork Source Instruction](../instructions/unitofwork-source.instructions.md)
- [Library Refactoring Instruction](../instructions/library-refactoring.instructions.md)