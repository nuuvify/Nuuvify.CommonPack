# Nuuvify.CommonPack.MftMailbox

[![PR Validation](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml)
[![Publish and Release](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml/badge.svg?branch=main)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml)
[![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.MftMailbox.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox/)
[![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.MftMailbox.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox/)

Núcleo de orquestração para integração com MFT Mailbox, com foco em:

- resolução de clientes por protocolo (factory)
- idempotência
- auditoria de transferência
- utilitários de resiliência e checksum

## Índice

- [Quando usar](#quando-usar)
- [Pacotes relacionados](#pacotes-relacionados)
- [Instalação](#instalação)
- [Configuração no DI](#configuração-no-di)
- [Configuração de opções](#configuração-de-opções)
- [Exemplo de uso](#exemplo-de-uso)
- [Componentes principais](#componentes-principais)
- [Boas práticas em produção](#boas-práticas-em-produção)
- [Compatibilidade](#compatibilidade)

## Quando usar

- Quando você quer padronizar o consumo de MFT entre múltiplos protocolos.
- Quando o projeto precisa de uma camada única de factory, idempotência e auditoria.
- Quando adapters SFTP/HTTPS devem ser plugáveis sem acoplamento no domínio.

## Pacotes relacionados

- Nuuvify.CommonPack.MftMailbox.Abstraction
- Nuuvify.CommonPack.MftMailbox.Sftp
- Nuuvify.CommonPack.MftMailbox.Http

## Instalação

```xml
<PackageReference Include="Nuuvify.CommonPack.MftMailbox" Version="x.x.x" />
```

## Configuração no DI

```csharp
using Nuuvify.CommonPack.MftMailbox;

builder.Services.AddMftMailboxCore(options =>
{
	options.MaxBatchSize = 500;
	options.MaxFileSizeBytes = 1_073_741_824;
	options.MaxDegreeOfParallelism = 4;
	options.FileTimeout = TimeSpan.FromMinutes(5);
	options.BatchTimeout = TimeSpan.FromMinutes(30);

	options.Retry.MaxRetries = 3;
	options.Retry.BaseDelay = TimeSpan.FromSeconds(2);
	options.Retry.MaxDelay = TimeSpan.FromSeconds(30);

	options.CircuitBreaker.FailureThreshold = 5;
	options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(60);
});
```

## Configuração de opções

### MftMailboxOptions

- MaxBatchSize: limite de itens por lote.
- MaxFileSizeBytes: limite por arquivo.
- MaxDegreeOfParallelism: concorrência máxima sugerida.
- FileTimeout: timeout por arquivo.
- BatchTimeout: timeout por lote.
- Retry: configuração de tentativas/transientes.
- CircuitBreaker: proteção contra indisponibilidade externa.

### Stores e sinks padrão

O pacote registra implementações padrão:

- IMftIdempotencyStore -> InMemoryMftIdempotencyStore
- ITransferAuditSink -> NullTransferAuditSink
- IMftClientFactory -> MftClientFactory

Você pode substituir por implementações próprias no seu projeto.

## Exemplo de uso

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

public sealed class MftStatusService
{
	private readonly IMftClientFactory _factory;

	public MftStatusService(IMftClientFactory factory)
	{
		_factory = factory;
	}

	public Task<TransferStatus?> GetAsync(string integrationKey, string itemId, CancellationToken ct)
	{
		var client = _factory.CreateStatusClient(MftProtocol.Https);
		return client.GetStatusAsync(integrationKey, itemId, ct);
	}
}
```

## Componentes principais

- MftMailboxSetup: extensão AddMftMailboxCore.
- MftClientFactory: resolve implementação por MftProtocol.
- InMemoryMftIdempotencyStore: referência para cenários simples e testes.
- RetryExecutor, ResilienceGate, ChecksumCalculator, IdempotencyKeyBuilder.

## Boas práticas em produção

- Troque InMemoryMftIdempotencyStore por store persistente.
- Troque NullTransferAuditSink por sink de observabilidade (log, fila ou banco).
- Ajuste retry/circuit breaker por SLA da integração.
- Defina limites de lote/arquivo compatíveis com throughput do ambiente.

## Compatibilidade

- Framework alvo: .NET 8
- Dependência de abstração: Nuuvify.CommonPack.MftMailbox.Abstraction
