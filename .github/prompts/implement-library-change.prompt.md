---
description: "Criar ou alterar código de um pacote Nuuvify.CommonPack com testes e documentação proporcionais à mudança."
name: "Implement Library Change"
argument-hint: "Pacote, objetivo, comportamento esperado, restrições e validação desejada"
agent: "agent"
model: "GPT-5 (copilot)"
---

Implemente a mudança pedida neste repositório seguindo estas regras:

- Comece pelo pacote dono do comportamento.
- Faça a menor mudança que resolva a causa raiz.
- Preserve contratos públicos e nomes estáveis sempre que possível.
- Atualize ou crie testes do comportamento alterado.
- Atualize documentação do pacote apenas se houver impacto público.
- Valide com o teste ou build mais estreito possível antes de ampliar o escopo.

Se a tarefa estiver concentrada em `UnitOfWork`, `BackgroundService` ou `Security`, prefira o prompt especializado do pacote correspondente em `.github/prompts/`.

Checklist de saída:
1. Mudança implementada.
2. Testes ajustados ou criados.
3. Validação executada.
4. Riscos residuais declarados, se existirem.

Use também:
- [Nuuvify Library Source](../instructions/library-source.instructions.md)
- [Nuuvify Library Tests](../instructions/library-tests.instructions.md)
