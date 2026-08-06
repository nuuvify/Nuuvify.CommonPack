---
description: "Planejar uma mudança em biblioteca Nuuvify.CommonPack antes de implementar, com escopo, riscos, testes e impacto documental."
name: "Plan Library Change"
argument-hint: "Pacote, objetivo, restrições, arquivos ou símbolos envolvidos"
agent: "plan"
model: "GPT-5 (copilot)"
---

Crie um plano enxuto para a mudança solicitada neste repositório de bibliotecas .NET.

Contexto obrigatório:
- Use a arquitetura do repositório e as convenções já existentes.
- Respeite compatibilidade de API pública e SemVer.
- Considere Clean Architecture, SOLID e mínimo acoplamento.

Entregue:
1. Pacote dono da mudança e fronteiras afetadas.
2. Hipótese local de implementação.
3. Menor conjunto de arquivos a alterar.
4. Testes a criar ou atualizar.
5. Documentação a atualizar, se houver impacto público.
6. Riscos de breaking change ou regressão.

Se a mudança estiver concentrada em `UnitOfWork`, `BackgroundService` ou `Security`, prefira o prompt especializado do pacote correspondente em `.github/prompts/`.

Referências:
- [Copilot Instructions](../copilot-instructions.md)
- [Arquitetura](../../docs/maintainers/architecture.md)
- [Guia de IA](../../docs/ai/README.md)
- [UnitOfWork](../../src/Nuuvify.CommonPack.UnitOfWork/README.md)
- [BackgroundService](../../src/Nuuvify.CommonPack.BackgroundService/README.md)
- [Security](../../src/Nuuvify.CommonPack.Security/README.md)
