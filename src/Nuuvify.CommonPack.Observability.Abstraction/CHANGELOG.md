# Changelog

## [Não Lançado]

- Documentado o contrato neutro de `IOperationContextAccessor` para migração de `RequestConfiguration`.

## 2.8.0

- Adicionado o contrato neutro de contexto de operação para correlação, trace e metadata.
- Definido `OperationContext` imutável e `IOperationContextAccessor` minimalista.
- Mantém o pacote sem dependência de ASP.NET Core ou hosting.
