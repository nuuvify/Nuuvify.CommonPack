---
name: "Nuuvify - GitHub PR Workflow"
description: "Use when the work involves GitHub pull request comments obtained through API, including code review comments, unresolved threads, bot findings and review-guided fixes."
---

# Workflow de comentarios de PR do GitHub

- Se o usuario informar a URL completa do PR, extraia automaticamente owner, repo e prNumber antes de executar qualquer coleta.
- Se a URL completa do PR nao for informada, pergunte apenas os dados faltantes antes de executar qualquer coleta.
- Para coleta automatizada, prefira API via script versionado do repositorio em vez de depender de browser ou copy/paste manual.
- Use `GITHUB_LZOCATELI_TOKEN` (preferencial), `GH_TOKEN` ou `GITHUB_TOKEN` para autenticacao. Nunca solicite ou exponha o valor.
- Agrupe comentarios repetidos por regra e arquivo antes de editar o codigo.
- Priorize risco funcional, regressao, seguranca e quebra de contrato antes de estilo.
- Em comentarios massivos de analyzer, prefira uma estrategia consistente por padrao em lote, com escopo restrito aos arquivos comentados.
- Quando houver padrao recorrente de analyzer no PR, atualize instrucoes em `.github/instructions/` para prevenir reincidencia.
- Depois da primeira edicao substantiva, rode validacao focada no slice alterado.
- Ao encerrar, informe claramente quais comentarios foram tratados, quais validacoes foram executadas e quais itens ficaram pendentes.
