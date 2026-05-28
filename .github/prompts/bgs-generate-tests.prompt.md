---
description: "Gerar ou atualizar testes para Nuuvify.CommonPack.BackgroundService com foco em cancelamento, concorrência, fluxo de falha, abandon/dead letter e diagnósticos."
name: "BGS Generate Tests"
argument-hint: "Arquivo, símbolo, cenário de mensagem e categoria Unit/Integration"
agent: "agent"
model: "GPT-5 (copilot)"
---

Crie ou ajuste testes para `Nuuvify.CommonPack.BackgroundService`.

Verifique principalmente:
- cancelamento
- concorrência
- falha controlada
- comportamento de abandon e dead letter
- propriedades de diagnóstico e telemetria observáveis

Use também:
- [BackgroundService Source Instruction](../instructions/backgroundservice-source.instructions.md)
- [Library Tests Instruction](../instructions/library-tests.instructions.md)