---
description: "Use when creating, updating, or refactoring tests in test/** for Nuuvify.CommonPack. Covers xUnit, Moq, Bogus, trait conventions, deterministic tests, fixtures, integration boundaries, and regression coverage."
name: "Nuuvify Library Tests"
applyTo: "test/**/*.cs, test/**/*.csproj"
---

# Diretrizes para testes

- Use xUnit como base.
- Classifique testes com `Trait("Category", "Unit")` ou `Trait("Category", "Integration")`.
- Prefira `Unit` por padrão; suba para `Integration` apenas quando validar a colaboração entre componentes exigir isso.
- Testes devem ser determinísticos, rápidos e isolados.
- Use Moq para dependências comportamentais e Bogus para dados de entrada quando isso reduzir ruído.
- Ao criar novo conjunto de testes para entidades ou modelos reutilizados, prefira separar faker e classe de testes para evitar duplicação.
- Cubra cenário feliz, bordas relevantes e regressões do comportamento alterado.
- Não replique a implementação nas asserts; valide contrato observável.

## Testes de integração

- Se o cenário exigir infraestrutura web, use `CustomWebApplicationFactory`.
- Quando houver chamadas HTTP externas, prefira mocks controlados, como `HttpTest` quando já fizer parte do padrão do projeto.
- Use banco em memória ou sandbox controlado quando a intenção for integração local, não dependência externa real.

## Validação mínima

- Toda mudança de comportamento precisa de teste novo ou ajuste explícito do teste existente.
- Se um bug foi corrigido, inclua pelo menos um teste de regressão que falha sem a correção.
