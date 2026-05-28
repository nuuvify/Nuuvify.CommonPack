---
name: library-maintenance
description: 'Create, change, refactor, test, review, or document Nuuvify.CommonPack .NET libraries with minimal token usage. Use for package-focused implementation, API-safe refactoring, regression tests, PR reviews, and documentation updates guided by Clean Architecture, SOLID, SemVer, and repo conventions.'
argument-hint: 'Pacote, tarefa, objetivo e restrições'
user-invocable: true
---

# Library Maintenance

## Quando usar

Use esta skill quando a tarefa envolver um ou mais destes objetivos em bibliotecas do repositório:

- criar ou alterar comportamento em um pacote
- refatorar preservando contrato
- adicionar ou ajustar testes
- revisar PR, diff ou arquivo
- atualizar README, CHANGELOG ou documentação de pacote

Quando a tarefa estiver claramente concentrada em `UnitOfWork`, `BackgroundService` ou `Security`, prefira os prompts especializados do pacote em `.github/prompts/` antes de ampliar contexto em prompts genéricos.

Para esses pacotes, a especialização já cobre planejamento, implementação, refatoração, testes, revisão e documentação.

## Seleção de modelo

- Use `GPT-5` como padrão para implementação, correção, refatoração e geração de testes.
- Prefira `Claude Sonnet 4.5` para revisão ampla ou síntese documental longa, quando disponível.
- Se o modelo preferido não estiver disponível, mantenha o fluxo em `GPT-5` em vez de aumentar escopo ou contexto.

## Agentes especializados

- Use `library-reviewer` para revisão somente leitura com foco em achados e sem risco de edição acidental.
- Use `library-refactorer` para refatoração incremental com edição e validação local, mas sem web ou subagentes.

## Disciplina de tokens

- Comece por um pacote, arquivo, símbolo ou teste específico.
- Leia apenas o código dono do comportamento, o teste vizinho e a documentação local do pacote.
- Não carregue documentação global desnecessária se o pacote já tiver `README.md` ou `CHANGELOG.md` suficientes.
- Prefira instruções sob demanda e prompts específicos a uma conversa longa e genérica.

## Procedimento

1. Identifique o pacote dono da mudança.
2. Escolha o fluxo adequado no guia em [playbooks](./references/playbooks.md).
3. Faça a menor mudança possível para validar a hipótese local.
4. Rode o teste, build ou revisão mais estreita compatível com a mudança.
5. Atualize documentação apenas se houver efeito público.

## Arquivos de apoio

- [Copilot Instructions](../../copilot-instructions.md)
- [Library Source Instruction](../../instructions/library-source.instructions.md)
- [Library Tests Instruction](../../instructions/library-tests.instructions.md)
- [Package Docs Instruction](../../instructions/package-docs.instructions.md)
- [Library Reviewer Agent](../../agents/library-reviewer.agent.md)
- [Library Refactorer Agent](../../agents/library-refactorer.agent.md)
- [Playbooks](./references/playbooks.md)
