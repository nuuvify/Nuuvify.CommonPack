# Nuuvify.CommonPack.Observability.Abstraction

Pacote neutro para contexto de operação sem acoplamento a ASP.NET Core, hosting ou infraestrutura específica.

## Visão geral

- `OperationContext` encapsula correlação, trace e metadados mínimos.
- `IOperationContextAccessor` fornece o ponto de acesso ao contexto atual.
- O pacote é adequado para uso em APIs, workers e fluxos assíncronos.
