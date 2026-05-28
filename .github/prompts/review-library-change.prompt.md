---
description: "Revisar mudanças em bibliotecas Nuuvify.CommonPack com foco em bugs, regressões, quebras de API, lacunas de teste e documentação."
name: "Review Library Change"
argument-hint: "PR, pacote, arquivo ou diff para revisar"
agent: "library-reviewer"
model: ["Claude Sonnet 4.5 (copilot)", "GPT-5 (copilot)"]
---

Faça uma revisão técnica da mudança solicitada.

Prioridades:
1. Bugs e regressões prováveis.
2. Quebras de contrato público ou incompatibilidade entre pacotes.
3. Testes ausentes ou frágeis.
4. Documentação pública desatualizada.
5. Riscos de acoplamento, complexidade ou manutenção.

Se a revisão estiver concentrada em `UnitOfWork`, `BackgroundService` ou `Security`, prefira o prompt especializado do pacote correspondente em `.github/prompts/`.

Formato esperado:
- Liste achados primeiro, em ordem de severidade.
- Seja específico sobre impacto e evidência.
- Se não houver achados, diga isso explicitamente e aponte riscos residuais.

Use também:
- [Nuuvify Library Review](../instructions/library-review.instructions.md)
