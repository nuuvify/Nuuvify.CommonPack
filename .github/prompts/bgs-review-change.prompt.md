---
description: "Revisar mudanças em Nuuvify.CommonPack.BackgroundService com foco em perda de mensagem, retry indevido, lock renewal, telemetria, diagnósticos e semântica de falha."
name: "BGS Review Change"
argument-hint: "PR, diff, arquivo ou símbolo para revisar"
agent: "library-reviewer"
model: ["Claude Sonnet 4.5 (copilot)", "GPT-5 (copilot)"]
---

Faça uma revisão técnica da mudança em `Nuuvify.CommonPack.BackgroundService`.

Priorize:
1. perda de mensagem, retry indevido ou fluxo incorreto de falha
2. problemas de cancelamento, concorrência ou lock renewal
3. diagnósticos e telemetria quebrados
4. lacunas de teste
5. risco operacional para execução contínua

Use também:
- [BackgroundService Source Instruction](../instructions/backgroundservice-source.instructions.md)
- [Library Review Instruction](../instructions/library-review.instructions.md)