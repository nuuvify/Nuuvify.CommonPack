# Changelog

## [Não Lançado]

- Documentada a adoção de `OperationContextScope` para requests, mensagens e jobs.

## 2.8.0

- Adicionada a implementação de contexto de operação isolada por `AsyncLocal`.
- Incluído `OperationContextScope` para preservação do contexto anterior em escopos aninhados.
- Integração mínima com `System.Diagnostics.Activity` para uso em workers e requests.
