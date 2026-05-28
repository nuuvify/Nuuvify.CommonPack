---
description: "Use when creating, altering, fixing, or refactoring code in Nuuvify.CommonPack.Security, Nuuvify.CommonPack.Security.JwtCredentials, or Nuuvify.CommonPack.Security.JwtStore.Ef. Covers token validation, claims, secure defaults, secret handling, JWT flows, authentication contracts, and SemVer-safe security changes."
name: "Nuuvify Security Source"
applyTo: "src/Nuuvify.CommonPack.Security/**/*.cs, src/Nuuvify.CommonPack.Security.JwtCredentials/**/*.cs, src/Nuuvify.CommonPack.Security.JwtStore.Ef/**/*.cs, src/Nuuvify.CommonPack.Security/**/*.csproj, src/Nuuvify.CommonPack.Security.JwtCredentials/**/*.csproj, src/Nuuvify.CommonPack.Security.JwtStore.Ef/**/*.csproj"
---

# Diretrizes para Security

- Preserve defaults seguros; toda flexibilização de validação precisa de motivo claro e teste explícito.
- Trate tokens, claims, segredos, chaves e material sensível como superfície de alto risco.
- Não enfraqueça validações de autenticação, autorização ou expiração sem avaliar impacto público e de segurança.
- Considere mudanças em autenticação e JWT como candidatas a breaking change observável.
- Prefira mensagens de erro previsíveis sem vazar detalhes sensíveis.

## Cuidados de implementação

- Preserve compatibilidade de contratos usados por consumidores e middlewares.
- Evite espalhar lógica de segurança entre helpers genéricos sem dono claro.
- Se a documentação do pacote estiver incompleta, use código e testes como fonte primária antes de alterar contrato público.

## Validação prioritária

- validação de token
- claims e autorização
- cenários inválidos ou expirados
- ausência de vazamento de segredo ou detalhe sensível