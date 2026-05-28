---
description: "Gerar ou atualizar testes para Nuuvify.CommonPack.UnitOfWork ou UnitOfWork.Abstraction com foco em queries encadeadas, filtros dinâmicos, paginação, ordenação e regressão de tradução LINQ/EF."
name: "UoW Generate Tests"
argument-hint: "Arquivo, símbolo, cenário, provider ou categoria Unit/Integration"
agent: "agent"
model: "GPT-5 (copilot)"
---

Crie ou ajuste testes para `Nuuvify.CommonPack.UnitOfWork` ou `Nuuvify.CommonPack.UnitOfWork.Abstraction`.

Verifique principalmente:
- queries encadeadas
- filtros dinâmicos
- ordenação e paginação
- regressões de tradução LINQ/EF
- preservação de contratos públicos observáveis

Use também:
- [UnitOfWork Source Instruction](../instructions/unitofwork-source.instructions.md)
- [Library Tests Instruction](../instructions/library-tests.instructions.md)