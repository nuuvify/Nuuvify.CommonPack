---
description: "Revisar mudanças em Nuuvify.CommonPack.Security, Security.JwtCredentials ou Security.JwtStore.Ef com foco em defaults inseguros, validação, segredos, claims e contratos de autenticação."
name: "Security Review Change"
argument-hint: "PR, diff, arquivo ou símbolo para revisar"
agent: "library-reviewer"
model: ["Claude Sonnet 4.5 (copilot)", "GPT-5 (copilot)"]
---

Faça uma revisão técnica da mudança em `Nuuvify.CommonPack.Security`, `Nuuvify.CommonPack.Security.JwtCredentials` ou `Nuuvify.CommonPack.Security.JwtStore.Ef`.

Priorize:
1. defaults inseguros ou validações enfraquecidas
2. vazamento de segredo ou detalhe sensível
3. regressões em autenticação, claims, expiração ou autorização
4. quebra de contrato público para consumidores
5. lacunas de teste e documentação

Use também:
- [Security Source Instruction](../instructions/security-source.instructions.md)
- [Library Review Instruction](../instructions/library-review.instructions.md)