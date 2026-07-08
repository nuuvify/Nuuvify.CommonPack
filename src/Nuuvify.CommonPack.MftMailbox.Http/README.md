# Nuuvify.CommonPack.MftMailbox.Http

[![PR Validation](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml)
[![Publish and Release](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml/badge.svg?branch=main)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml)
[![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.MftMailbox.Http.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox.Http/)
[![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.MftMailbox.Http.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox.Http/)

Adapter HTTPS Mailbox para integração MFT com suporte a:

- upload/download por streaming
- polling de status com backoff
- confirmação ACK/NACK por API
- tratamento resiliente para falhas transientes

## Índice

- [Quando usar](#quando-usar)
- [Dependências](#dependências)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Registro no DI](#registro-no-di)
- [Exemplo de envio](#exemplo-de-envio)
- [Exemplo de status](#exemplo-de-status)
- [Exemplo de ACK/NACK](#exemplo-de-acknack)
- [Segurança](#segurança)
- [Troubleshooting](#troubleshooting)

## Quando usar

- Quando o provedor MFT expõe Mailbox via API HTTPS.
- Quando o fluxo exige status assíncrono com polling controlado.
- Quando ACK/NACK é confirmado por endpoint HTTP.

## Dependências

- Nuuvify.CommonPack.MftMailbox
- Nuuvify.CommonPack.MftMailbox.Abstraction
- Microsoft.Extensions.Http

## Instalação

```xml
<PackageReference Include="Nuuvify.CommonPack.MftMailbox.Http" Version="x.x.x" />
```

## Configuração

### HttpMftMailboxOptions

- BaseUrl
- UploadPath
- DownloadPath
- ListPath
- StatusPath
- AckNackPath
- BearerToken
- UseMutualTls
- StatusPollingMaxAttempts
- StatusPollingBaseDelay

## Registro no DI

```csharp
using Nuuvify.CommonPack.MftMailbox;
using Nuuvify.CommonPack.MftMailbox.Http;

builder.Services.AddMftMailboxCore();

builder.Services.AddMftMailboxHttp(options =>
{
	options.BaseUrl = "https://mft.example.com";
	options.UploadPath = "/mailbox/upload";
	options.DownloadPath = "/mailbox/download";
	options.ListPath = "/mailbox/list";
	options.StatusPath = "/mailbox/status";
	options.AckNackPath = "/mailbox/acknack";
	options.BearerToken = "token-via-secret-provider";
	options.StatusPollingMaxAttempts = 10;
	options.StatusPollingBaseDelay = TimeSpan.FromSeconds(2);
});
```

## Exemplo de envio

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

public sealed class HttpOutboundService
{
	private readonly IMftClientFactory _factory;

	public HttpOutboundService(IMftClientFactory factory)
	{
		_factory = factory;
	}

	public async Task<TransferItemResult> SendAsync(Stream payload, CancellationToken ct)
	{
		var client = _factory.CreateTransferClient(MftProtocol.Https);

		var envelope = new TransferEnvelope
		{
			IntegrationKey = "erp-http",
			CorrelationId = Guid.NewGuid().ToString("N"),
			Protocol = MftProtocol.Https,
			Items = new List<TransferItem>
			{
				new()
				{
					ItemId = Guid.NewGuid().ToString("N"),
					FileName = "orders.json",
					ContentFactory = _ => Task.FromResult(payload)
				}
			}
		};

		return await client.SendSingleAsync(envelope, ct);
	}
}
```

## Exemplo de status

```csharp
var statusClient = _factory.CreateStatusClient(MftProtocol.Https);
var status = await statusClient.GetStatusAsync("erp-http", "item-01", ct);
```

## Exemplo de ACK/NACK

```csharp
var ackClient = _factory.CreateAckNackClient(MftProtocol.Https);

await ackClient.AckOrNackAsync(new AckNackCommand
{
	IntegrationKey = "erp-http",
	CorrelationId = Guid.NewGuid().ToString("N"),
	ItemId = "item-01",
	FileName = "orders.json",
	Protocol = MftProtocol.Https,
	Decision = AckNackType.Nack,
	Reason = "checksum mismatch"
}, ct);
```

## Segurança

- Use BearerToken obtido de secret manager.
- Se necessário, habilite mTLS na infraestrutura HTTP.
- Não registre payload sensível em logs.
- Restrinja permissões e escopo do token por integração.

## Troubleshooting

- 401/403: validar token, escopo e relógio do ambiente.
- Timeout: revisar BaseUrl, proxy, timeout e políticas de retry.
- Polling sem conclusão: ajustar StatusPollingMaxAttempts/base delay e validar endpoint de status.
- Falha de desserialização: validar contrato de resposta do provedor MFT.
