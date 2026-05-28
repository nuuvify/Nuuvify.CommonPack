---
description: "Refatorar Nuuvify.CommonPack.Security, Security.JwtCredentials ou Security.JwtStore.Ef com segurança para validação, claims, JWT, autenticação e segredos."
name: "Security Refactor Code"
argument-hint: "Símbolo, arquivo, cheiro de código e teste alvo"
agent: "library-refactorer"
model: "GPT-5 (copilot)"
---

Refatore o trecho solicitado em `Nuuvify.CommonPack.Security`, `Nuuvify.CommonPack.Security.JwtCredentials` ou `Nuuvify.CommonPack.Security.JwtStore.Ef`.

Cuidados obrigatórios:
- preserve invariantes de validação e autenticação
- não simplifique criptografia, emissão ou armazenamento de token sem evidência e teste
- preserve contratos públicos usados por consumidores e middlewares
- evite vazamento de detalhes sensíveis em erros e logs

Use também:
- [Security Source Instruction](../instructions/security-source.instructions.md)
- [Library Refactoring Instruction](../instructions/library-refactoring.instructions.md)