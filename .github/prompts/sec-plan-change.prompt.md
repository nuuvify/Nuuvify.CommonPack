---
description: "Planejar mudança em Nuuvify.CommonPack.Security, Security.JwtCredentials ou Security.JwtStore.Ef com foco em validação de token, claims, defaults seguros, JWT e risco de breaking change."
name: "Security Plan Change"
argument-hint: "Objetivo, fluxo de autenticação, símbolo ou arquivo e risco conhecido"
agent: "plan"
model: "GPT-5 (copilot)"
---

Crie um plano enxuto para a mudança solicitada em `Nuuvify.CommonPack.Security`, `Nuuvify.CommonPack.Security.JwtCredentials` ou `Nuuvify.CommonPack.Security.JwtStore.Ef`.

Foque em:
- validação de token e claims
- defaults seguros
- JWT, autenticação e autorização
- risco de breaking change para consumidores
- risco de vazamento de segredo ou de detalhe sensível

Entregue:
1. ponto do fluxo onde a regra é realmente decidida
2. hipótese local de implementação
3. menor conjunto de arquivos a alterar
4. validação ou teste mais estreito
5. riscos de segurança, regressão ou SemVer

Referências:
- [Security README](../../src/Nuuvify.CommonPack.Security/README.md)
- [Security Source Instruction](../instructions/security-source.instructions.md)