---
description: "Gerar ou atualizar testes xUnit para mudanças em bibliotecas Nuuvify.CommonPack com foco em regressão e determinismo."
name: "Generate Library Tests"
argument-hint: "Arquivo, símbolo, cenário, categoria Unit ou Integration"
agent: "agent"
model: "GPT-5 (copilot)"
---

Crie ou ajuste testes para o comportamento informado.

Regras:
- Use xUnit, Moq e Bogus quando fizer sentido.
- Prefira testes pequenos, determinísticos e orientados a contrato.
- Classifique com `Trait("Category", "Unit")` ou `Trait("Category", "Integration")`.
- Inclua regressão para bugs corrigidos.
- Não replique a implementação nas asserts.

Entregue testes prontos para rodar no padrão do repositório.

Se o teste estiver concentrado em `UnitOfWork`, `BackgroundService` ou `Security`, prefira o prompt especializado do pacote correspondente em `.github/prompts/`.

Use também:
- [Nuuvify Library Tests](../instructions/library-tests.instructions.md)
- [Testes do repositório](../../docs/contributing/testing.md)
