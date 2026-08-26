# Nuuvify.CommonPack.Observability.Abstraction

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_Nuuvify.CommonPack&metric=alert_status)](https://sonarcloud.io/project/overview?id=nuuvify_Nuuvify.CommonPack)

[![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.Observability.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Observability.Abstraction)
[![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.Observability.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.Observability.Abstraction)

Contrato neutro para contexto de operação em bibliotecas, Application e Domain
que precisam conhecer correlação e rastreio sem depender de ASP.NET Core,
hosting, logging, banco de dados ou outro provedor de infraestrutura.

## Índice

- [Quando usar](#quando-usar)
- [O que resolve](#o-que-resolve)
- [Instalação](#instalação)
- [Exemplo de uso](#exemplo-de-uso)
- [Limites do contrato](#limites-do-contrato)
- [Compatibilidade](#compatibilidade)
- [Troubleshooting](#troubleshooting)

## Quando usar

Use este pacote quando uma camada neutra precisar receber ou consultar o
contexto de uma operação, mas não deve referenciar `HttpContext`,
`IConfiguration`, `ILogger`, ASP.NET Core ou APIs específicas do host.

É adequado para contratos compartilhados entre API, worker, jobs e adaptadores
de mensageria. Para registrar o contexto e abrir escopos em runtime, use também
`Nuuvify.CommonPack.Observability`.

## O que resolve

- define `OperationContext` imutável;
- padroniza `CorrelationId`, `TraceId`, `OperationId` e metadata técnica;
- fornece `IOperationContextAccessor` como dependência pequena e testável;
- mantém as regras de negócio independentes do mecanismo de transporte.

## Instalação

```xml
<PackageReference Include="Nuuvify.CommonPack.Observability.Abstraction" Version="2.8.0" />
```

## Exemplo de uso

```csharp
using Nuuvify.CommonPack.Observability.Abstraction;

public sealed class AuditEntry
{
    public AuditEntry(IOperationContextAccessor contextAccessor)
    {
        CorrelationId = contextAccessor.Current.CorrelationId;
        OperationId = contextAccessor.Current.OperationId;
    }

    public string CorrelationId { get; }

    public string OperationId { get; }
}
```

O pacote de abstração não decide como o contexto é armazenado. Essa decisão
pertence ao pacote de runtime ou ao adaptador do host.

## Limites do contrato

Não coloque headers, IPs, claims, tokens, secrets, payloads, `HttpContext`,
configuração ou tipos de logging em `OperationContext`. Metadata deve conter
somente informações técnicas mínimas e não sensíveis.

## Compatibilidade

- alvo: `netstandard2.1`;
- sem dependências de ASP.NET Core, hosting ou opções;
- pode ser referenciado por bibliotecas compatíveis com .NET Standard 2.1 e
aplicações .NET modernas.

## Troubleshooting

### O projeto precisa de `AsyncLocal`

Não implemente armazenamento no contrato. Referencie o pacote
`Nuuvify.CommonPack.Observability` no runtime que controla os escopos.

### Metadata contém dados sensíveis

Remova esses dados do contexto e transporte apenas identificadores técnicos ou
um identificador não sensível. O contrato não é um cofre nem um repositório de
claims.
