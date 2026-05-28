---
description: "Implementar mudança em Nuuvify.CommonPack.Security, Security.JwtCredentials ou Security.JwtStore.Ef preservando validação, claims, defaults seguros e contratos públicos de autenticação."
name: "Security Implement Change"
argument-hint: "Objetivo, comportamento esperado, símbolo ou arquivo e validação desejada"
agent: "agent"
model: "GPT-5 (copilot)"
---

Implemente a mudança pedida em `Nuuvify.CommonPack.Security`, `Nuuvify.CommonPack.Security.JwtCredentials` ou `Nuuvify.CommonPack.Security.JwtStore.Ef`.

Restrições principais:
- preserve defaults seguros e contratos de autenticação observáveis
- não enfraqueça validações de token, claims ou expiração sem teste explícito
- não exponha segredos nem detalhes sensíveis em logs, exceções ou respostas
- trate mudanças em JWT e autenticação como candidatas a breaking change

Checklist de saída:
1. mudança implementada
2. validação estreita executada
3. risco residual informado, se houver

Use também:
- [Security Source Instruction](../instructions/security-source.instructions.md)
- [Library Tests Instruction](../instructions/library-tests.instructions.md)