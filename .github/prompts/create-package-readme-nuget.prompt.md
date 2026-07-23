---
description: "Criar ou reescrever README de pacote em src/** no padrão nuget.org, com exemplos aderentes à API pública atual."
name: "Create Package README NuGet"
argument-hint: "Pacote alvo, cenário de uso, público-alvo, nível de detalhe e exemplos desejados"
agent: "agent"
model: ["GPT-5 (copilot)", "Claude Sonnet 4.5 (copilot)"]
---

Crie ou atualize o `README.md` do pacote informado em `src/**` para ficar pronto para publicação no nuget.org.

Objetivos:
- Produzir onboarding completo para consumidor externo.
- Garantir consistência entre README do pacote, Readme raiz e CHANGELOG do pacote.
- Usar exemplos curtos, funcionais e alinhados aos contratos públicos atuais.

Regras de execução:
- Antes de escrever, leia a API pública real (interfaces, classes e métodos expostos).
- Não invente símbolos nem exemplos incompatíveis com o código existente.
- Use placeholders seguros para segredos e endpoints.
- Evite texto genérico e marketing desnecessário.

Estrutura mínima esperada:
1. Título do pacote e badges.
2. Descrição curta de propósito.
3. Índice.
4. Quando usar.
5. Dependências/pacotes relacionados.
6. Instalação.
7. Configuração (quando aplicável).
8. Exemplos de uso práticos.
9. Boas práticas/segurança (quando aplicável).
10. Troubleshooting.
11. Compatibilidade.

Se houver impacto público novo durante a atualização do README:
- Atualize também `src/<Pacote>/CHANGELOG.md` com linguagem voltada ao consumidor.

Checklist de saída:
1. README atualizado no padrão nuget.org.
2. Exemplos aderentes ao código público atual.
3. Consistência com documentação central e changelog.
4. Riscos ou limitações declarados (se houver).

Use também:
- [Nuuvify Package Docs](../instructions/package-docs.instructions.md)
- [Nuuvify Package README NuGet](../instructions/package-readme-nuget.instructions.md)
