---
description: "Refatorar código de biblioteca Nuuvify.CommonPack preservando comportamento, reduzindo complexidade e mantendo validação estreita."
name: "Refactor Library Code"
argument-hint: "Pacote, símbolo ou arquivo, cheiro de código, restrições e teste alvo"
agent: "library-refactorer"
model: "GPT-5 (copilot)"
---

Refatore o trecho solicitado com foco em clareza, coesão e baixo risco.

Objetivos:
- Preservar comportamento observável.
- Reduzir complexidade, duplicação ou acoplamento.
- Evitar abstrações prematuras.
- Revalidar com o teste mais estreito disponível.

Se a refatoração estiver concentrada em `UnitOfWork`, `BackgroundService` ou `Security`, prefira o prompt especializado do pacote correspondente em `.github/prompts/`.

Entregue uma refatoração incremental, sem ampliar escopo desnecessariamente.

Use também:
- [Nuuvify Library Refactoring](../instructions/library-refactoring.instructions.md)
- [Nuuvify Library Source](../instructions/library-source.instructions.md)
