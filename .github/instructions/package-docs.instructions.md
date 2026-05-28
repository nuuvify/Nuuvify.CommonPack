---
description: "Use when updating README, CHANGELOG, package docs, contribution docs, or release notes for Nuuvify.CommonPack. Covers concise package documentation, public API communication, migration notes, and token-efficient AI-generated docs."
name: "Nuuvify Package Docs"
applyTo: "Readme.md, docs/**/*.md, src/**/README.md, src/**/CHANGELOG.md"
---

# Diretrizes para documentação

- Documente comportamento público, pré-requisitos, exemplos e limites relevantes; omita detalhes internos sem valor para o consumidor.
- Em `README.md` de pacote, priorize: propósito, instalação, configuração mínima, exemplo de uso e observações de compatibilidade.
- Em `CHANGELOG.md`, registre apenas o que um consumidor precisa saber para atualizar com segurança.
- Se houver breaking change, inclua impacto, migração e pacote afetado.
- Use linguagem direta, exemplos curtos e blocos de código mínimos.
- Não duplique explicações extensas já cobertas em outro arquivo; prefira apontar o local certo.

## Documentação produzida com IA

- Reduza texto genérico e marketing excessivo.
- Preserve consistência de termos entre `Readme.md`, `docs/` e documentação dos pacotes.
- Quando a mudança for local a um pacote, atualize o `README.md` e o `CHANGELOG.md` daquele pacote antes de tocar documentação global.
