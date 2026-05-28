---
description: "Refatorar Nuuvify.CommonPack.BackgroundService com segurança para concorrência, cancelamento, lock renewal, telemetria e diagnósticos de falha."
name: "BGS Refactor Code"
argument-hint: "Símbolo, arquivo, cheiro de código e teste alvo"
agent: "library-refactorer"
model: "GPT-5 (copilot)"
---

Refatore o trecho solicitado em `Nuuvify.CommonPack.BackgroundService`.

Cuidados obrigatórios:
- preserve semântica de processamento e falha da mensagem
- não altere fluxo de lock, retry, abandon ou dead letter sem validação explícita
- preserve diagnósticos e telemetria observáveis
- revalide com o cenário mais estreito possível

Use também:
- [BackgroundService Source Instruction](../instructions/backgroundservice-source.instructions.md)
- [Library Refactoring Instruction](../instructions/library-refactoring.instructions.md)