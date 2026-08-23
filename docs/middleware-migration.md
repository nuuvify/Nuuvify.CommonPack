# Guia de migração do contexto de operação

## Objetivo

Substituir o uso operacional de `RequestConfiguration` por contexto de operação
isolado, preservando as APIs legadas durante a linha 2.x.

## Repositórios e HTTP

Use `IOperationContextAccessor` no construtor do adapter ou repositório. Abra um
`OperationContextScope` no limite da requisição, mensagem ou job e leia o
correlation ID de `accessor.Current?.CorrelationId`.

Para `BaseRepository`, prefira o construtor que recebe:

```csharp
base(unitOfWork, userAuthenticated, mapper, operationContextAccessor, cache)
```

Para as bases HTTP, prefira o construtor que recebe
`IOperationContextAccessor`. Os construtores que recebem `RequestConfiguration`
continuam disponíveis para consumidores antigos, mas estão marcados com
`Obsolete(error: false)`.

## Mensagens

Gere o correlation ID no início de cada mensagem quando o protocolo não fornecer
um valor. Não grave o valor em propriedades estáticas, singleton ou em uma
instância compartilhada entre execuções concorrentes.

```csharp
var correlationId = string.IsNullOrWhiteSpace(message.CorrelationId)
    ? Guid.NewGuid().ToString("N")
    : message.CorrelationId;
```

## Domínio

Não injete `IConfiguration`, `IOptions`, hosting ou `RequestConfiguration` no
Domain. Resolva configuração no composition root e entregue ao Domain somente
valores explícitos ou objetos próprios do caso de uso.

## Compatibilidade

Não remova assinaturas públicas na linha 2.x. Migre consumidores conhecidos,
compile com warnings visíveis e só então marque uma API legada com
`[Obsolete(message, error: false)]`. A remoção física fica reservada para a
próxima major.
