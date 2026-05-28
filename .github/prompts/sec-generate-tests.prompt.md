---
description: "Gerar ou atualizar testes para Nuuvify.CommonPack.Security, Security.JwtCredentials ou Security.JwtStore.Ef com foco em token, claims, expiração, autorização negada e ausência de vazamento sensível."
name: "Security Generate Tests"
argument-hint: "Arquivo, símbolo, cenário de autenticação e categoria Unit/Integration"
agent: "agent"
model: "GPT-5 (copilot)"
---

Crie ou ajuste testes para `Nuuvify.CommonPack.Security`, `Nuuvify.CommonPack.Security.JwtCredentials` ou `Nuuvify.CommonPack.Security.JwtStore.Ef`.

Verifique principalmente:
- validação de token
- claims e autorização
- cenários inválidos ou expirados
- defaults seguros
- ausência de vazamento de segredo ou detalhe sensível

Use também:
- [Security Source Instruction](../instructions/security-source.instructions.md)
- [Library Tests Instruction](../instructions/library-tests.instructions.md)