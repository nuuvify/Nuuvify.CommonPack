---
description: "Use when creating, altering, fixing, or refactoring code in Nuuvify.CommonPack.BackgroundService. Covers Service Bus message handling, concurrency, cancellation, lock renewal, abandon versus dead letter behavior, telemetry, diagnostics, and failure semantics."
name: "Nuuvify BackgroundService Source"
applyTo: "src/Nuuvify.CommonPack.BackgroundService/**/*.cs, src/Nuuvify.CommonPack.BackgroundService/**/*.csproj"
---

# Diretrizes para BackgroundService

- Preserve a semântica de processamento de mensagens antes de melhorar estrutura interna.
- Trate cancelamento, concorrência, lock renewal e comportamento de falha como partes críticas do contrato do pacote.
- Não altere sem evidência a decisão entre abandon e dead letter quando isso afetar operação observável.
- Preserve correlação, diagnóstico e telemetria emitidos para troubleshooting.
- Prefira mudanças locais no fluxo de processamento e no tratamento de erro, evitando espalhar responsabilidade entre muitas classes artificiais.

## Cuidados de implementação

- Verifique se exceções continuam gerando logs e metadados úteis.
- Evite mudanças que possam causar perda silenciosa de mensagens ou reprocessamento indevido.
- Preserve defaults seguros para execução contínua e operação em produção.

## Validação prioritária

- cancelamento
- concorrência
- fluxo de falha
- propriedades de diagnóstico
- comportamento de abandon e dead letter