# Nuuvify.CommonPack.MftMailbox.Abstraction

[![PR Validation](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml)
[![Publish and Release](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml/badge.svg?branch=main)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml)
[![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.MftMailbox.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox.Abstraction/)
[![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.MftMailbox.Abstraction.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox.Abstraction/)

Pacote de contratos e modelos compartilhados para integração com MFT Mailbox.

Este pacote define o contrato estável para envio, recebimento, consulta de status e confirmação ACK/NACK em fluxos de transferência de arquivos por SFTP e HTTPS.

## Índice

- [Quando usar](#quando-usar)
- [Pacotes relacionados](#pacotes-relacionados)
- [Instalação](#instalação)
- [Contratos principais](#contratos-principais)
- [Modelos principais](#modelos-principais)
- [Exemplo de uso no domínio/aplicação](#exemplo-de-uso-no-domínioaplicação)
- [Boas práticas](#boas-práticas)
- [Compatibilidade](#compatibilidade)

## Quando usar

- Quando o projeto terceiro precisa depender apenas de abstrações MFT.
- Quando você quer desacoplar domínio/aplicação da implementação concreta (SFTP/HTTPS).
- Quando o consumo deve ser testável por mocks sem dependência de infraestrutura.

## Pacotes relacionados

- Nuuvify.CommonPack.MftMailbox: núcleo com factory, idempotência e auditoria padrão.
- Nuuvify.CommonPack.MftMailbox.Sftp: implementação do adapter SFTP.
- Nuuvify.CommonPack.MftMailbox.Http: implementação do adapter HTTPS Mailbox.

## Instalação

```xml
<PackageReference Include="Nuuvify.CommonPack.MftMailbox.Abstraction" Version="x.x.x" />
```

## Contratos principais

### Interfaces de operação

- IMftTransferClient
	- SendSingleAsync
	- SendBatchAsync
- IMftInboundClient
	- ReceiveSingleAsync
	- ReceiveBatchAsync
- IMftStatusClient
	- GetStatusAsync
- IAckNackClient
	- AckOrNackAsync

### Interface de fábrica

- IMftClientFactory
	- CreateTransferClient
	- CreateInboundClient
	- CreateStatusClient
	- CreateAckNackClient

### Extensões de infraestrutura

- IMftIdempotencyStore
- ITransferAuditSink

## Modelos principais

- TransferEnvelope: contexto da operação (integração, correlação, protocolo e itens).
- TransferItem: unidade de transferência com metadados e ContentFactory para streaming.
- TransferItemResult e TransferBatchResult: resultado por item e consolidado por lote.
- TransferStatus: status observável da transferência.
- AckNackCommand: comando de confirmação (ACK/NACK).
- InboundTransferItem: stream de recebimento para consumo do cliente.
- TransferAuditEntry: trilha de auditoria por item/operação.

## Exemplo de uso no domínio/aplicação

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

public sealed class FileOutboundUseCase
{
		private readonly IMftClientFactory _factory;

		public FileOutboundUseCase(IMftClientFactory factory)
		{
				_factory = factory;
		}

		public async Task<TransferItemResult> ExecuteAsync(Stream stream, CancellationToken ct)
		{
				var transferClient = _factory.CreateTransferClient(MftProtocol.Sftp);

				var envelope = new TransferEnvelope
				{
						IntegrationKey = "orders",
						CorrelationId = Guid.NewGuid().ToString("N"),
						Protocol = MftProtocol.Sftp,
						Items = new List<TransferItem>
						{
								new()
								{
										ItemId = Guid.NewGuid().ToString("N"),
										FileName = "orders.csv",
										ContentFactory = _ => Task.FromResult(stream)
								}
						}
				};

				return await transferClient.SendSingleAsync(envelope, ct);
		}
}
```

## Boas práticas

- Mantenha domínio/aplicação dependente apenas deste pacote.
- Resolva implementação concreta no entrypoint via DI.
- Propague CancellationToken em toda cadeia de operação.
- Trate status e resultado por item para rastreabilidade.

## Compatibilidade

- Framework alvo: .NET 8
- Sem dependência de protocolo específico nesta camada.
