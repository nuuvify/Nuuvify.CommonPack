---
description: "Atualizar README, CHANGELOG ou documentação de pacote Nuuvify.CommonPack após mudanças de comportamento ou API pública."
name: "Update Package Docs"
argument-hint: "Pacote, mudança feita, público-alvo, nível de detalhe"
agent: "agent"
model: ["Claude Sonnet 4.5 (copilot)", "GPT-5 (copilot)"]
---

Atualize a documentação necessária para a mudança informada.

Objetivos:
- Explicar apenas o que o consumidor do pacote precisa saber.
- Manter exemplos curtos e corretos.
- Registrar impacto de upgrade quando houver breaking change.
- Evitar texto genérico e duplicação entre arquivos.

Se a documentação estiver concentrada em `UnitOfWork`, `BackgroundService` ou `Security`, prefira o prompt especializado do pacote correspondente em `.github/prompts/`.

Se a documentação do pacote estiver incompleta, use o código e os testes como fonte primária antes de escrever texto novo.

Considere, conforme o caso:
- `src/<Pacote>/README.md`
- `src/<Pacote>/CHANGELOG.md`
- `Readme.md`
- `docs/`

Use também:
- [Nuuvify Package Docs](../instructions/package-docs.instructions.md)
