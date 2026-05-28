---
description: "Implementar mudança em Nuuvify.CommonPack.BackgroundService preservando semântica de mensagem, concorrência, diagnóstico, lock e comportamento de falha."
name: "BGS Implement Change"
argument-hint: "Objetivo, comportamento esperado, símbolo ou arquivo e validação desejada"
agent: "agent"
model: "GPT-5 (copilot)"
---

Implemente a mudança pedida em `Nuuvify.CommonPack.BackgroundService`.

Restrições principais:
- preserve fluxo de mensagem e comportamento de falha
- trate cancelamento, lock renewal e concorrência como partes críticas do contrato
- preserve correlação, logs, telemetria e propriedades de diagnóstico
- prefira a menor mudança possível no ponto que decide o processamento

Checklist de saída:
1. mudança implementada
2. validação estreita executada
3. risco residual informado, se houver

Use também:
- [BackgroundService Source Instruction](../instructions/backgroundservice-source.instructions.md)
- [Library Tests Instruction](../instructions/library-tests.instructions.md)