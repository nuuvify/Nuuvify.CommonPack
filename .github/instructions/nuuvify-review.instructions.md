---
description: "Use when reviewing pull requests, code changes, refactorings, tests, or documentation in Nuuvify.CommonPack. Focus on bugs, regressions, public API breaks, missing tests, package boundary violations, security, and maintainability risks."
name: "Nuuvify Library Review"
---

# Diretrizes para revisão

- Priorize achados concretos: bug, regressão, quebra de contrato, lacuna de teste, risco de manutenção, acoplamento indevido ou risco de segurança.
- Revise com foco de biblioteca: API pública, compatibilidade, previsibilidade e custo de evolução importam mais que preferência estilística.
- Questione mudanças que atravessam muitos pacotes sem justificativa forte.
- Verifique se testes cobrem o comportamento modificado e se a documentação acompanha mudanças públicas.
- Em revisão de refatoração, cobre evidência de preservação de comportamento.

## Ordem de severidade

1. Quebra de contrato público, comportamento incorreto, falha de segurança ou regressão provável.
2. Lacunas de teste, ambiguidades de API, acoplamento ou dívida que aumenta risco futuro.
3. Melhorias menores de clareza, consistência ou documentação.
