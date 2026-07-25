# Nuuvify.CommonPack.MftMailbox.Sftp

[![PR Validation](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml)
[![Publish and Release](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml/badge.svg?branch=main)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml)
[![NuGet](https://img.shields.io/nuget/v/Nuuvify.CommonPack.MftMailbox.Sftp.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox.Sftp/)
[![Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.MftMailbox.Sftp.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox.Sftp/)

Adapter SFTP para MFT Mailbox com suporte a:

- envio e recebimento por streaming
- commit atômico em upload (tmp + rename)
- status por item
- confirmação ACK/NACK por metadado ou arquivo marcador

## Índice

- [Quando usar](#quando-usar)
- [Dependências](#dependências)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Registro no DI](#registro-no-di)
- [Exemplo de envio](#exemplo-de-envio)
- [Exemplo de recebimento](#exemplo-de-recebimento)
- [ACK/NACK](#acknack)
- [Mainframe (UTF-8 e EBCDIC)](#mainframe-utf-8-e-ebcdic)
- [Segurança](#segurança)
- [Troubleshooting](#troubleshooting)

## Quando usar

- Integrações de transferência de arquivo com servidores SFTP.
- Processamento de lote com rastreabilidade por item.
- Necessidade de comportamento resiliente para rede instável.

## Dependências

- Nuuvify.CommonPack.MftMailbox
- Nuuvify.CommonPack.MftMailbox.Abstraction
- SSH.NET

## Instalação

```xml
<PackageReference Include="Nuuvify.CommonPack.MftMailbox.Sftp" Version="x.x.x" />
```

## Configuração

### SftpMftMailboxOptions

- Host
- Port
- Username
- Password ou PrivateKeyPath/PrivateKeyPassphrase
- HostKeyFingerprint
- OutboundDirectory
- InboundDirectory
- ArchiveSuccessDirectory
- ArchiveErrorDirectory
- AckMarkerDirectory
- AckNackMode (`None`, `Metadata`, `MarkerFile`, `MetadataAndMarkerFile`)
- AckMarkerEncodingName (valores aceitos por `Encoding.GetEncoding`; exemplos: `utf-8`, `ibm037`)
- InboundFileOrdering (`None`, `FileNameAscending`, `FileNameDescending`)

## Registro no DI

```csharp
using Nuuvify.CommonPack.MftMailbox;
using Nuuvify.CommonPack.MftMailbox.Configuration;
using Nuuvify.CommonPack.MftMailbox.Sftp;
using Nuuvify.CommonPack.MftMailbox.Sftp.Configuration;

builder.Services.AddMftMailboxCore();

builder.Services.AddMftMailboxSftp(options =>
{
	options.Host = "sftp.example.local";
	options.Port = 22;
	options.Username = "integration-user";
	options.PrivateKeyPath = "/secrets/id_rsa";
	options.HostKeyFingerprint = "ab12cd34ef56...";
	options.OutboundDirectory = "/outbound";
	options.InboundDirectory = "/inbound";
	options.ArchiveSuccessDirectory = "/archive/success";
	options.ArchiveErrorDirectory = "/archive/error";
	options.AckMarkerDirectory = "/ack";
	options.AckNackMode = SftpAckNackMode.MetadataAndMarkerFile;
	options.AckMarkerEncodingName = "utf-8";
	options.InboundFileOrdering = InboundFileOrdering.FileNameAscending;
});
```

## Exemplo de envio

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

public sealed class SftpOutboundService
{
	private readonly IMftClientFactory _factory;

	public SftpOutboundService(IMftClientFactory factory)
	{
		_factory = factory;
	}

	public async Task<TransferItemResult> SendAsync(byte[] payload, CancellationToken ct)
	{
		var client = _factory.CreateTransferClient(MftProtocol.Sftp);

		var envelope = new TransferEnvelope
		{
			IntegrationKey = "supplier-a",
			CorrelationId = Guid.NewGuid().ToString("N"),
			Protocol = MftProtocol.Sftp,
			Items = new List<TransferItem>
			{
				new()
				{
					ItemId = Guid.NewGuid().ToString("N"),
					FileName = "invoice.csv",
					ContentFactory = _ => Task.FromResult<Stream>(new MemoryStream(payload))
				}
			}
		};

		return await client.SendSingleAsync(envelope, ct);
	}
}
```

## Exemplo de recebimento

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

public sealed class SftpInboundService
{
	private readonly IMftClientFactory _factory;

	public SftpInboundService(IMftClientFactory factory)
	{
		_factory = factory;
	}

	public async Task<IReadOnlyCollection<InboundTransferItem>> ReceiveAsync(CancellationToken ct)
	{
		var client = _factory.CreateInboundClient(MftProtocol.Sftp);

		var envelope = new TransferEnvelope
		{
			IntegrationKey = "supplier-a",
			CorrelationId = Guid.NewGuid().ToString("N"),
			Protocol = MftProtocol.Sftp
		};

		return await client.ReceiveBatchAsync(envelope, ct);
	}
}
```

## ACK/NACK

O adapter suporta quatro modos:

- None: não executa confirmação remota.
- Metadata: move arquivo para pasta de sucesso/erro.
- MarkerFile: cria arquivo marcador na pasta de ACK/NACK.
- MetadataAndMarkerFile: combina movimentação + marcador.

Exemplo:

```csharp
var client = _factory.CreateAckNackClient(MftProtocol.Sftp);

await client.AckOrNackAsync(new AckNackCommand
{
	IntegrationKey = "supplier-a",
	CorrelationId = Guid.NewGuid().ToString("N"),
	ItemId = "item-01",
	FileName = "invoice.csv",
	Protocol = MftProtocol.Sftp,
	Decision = AckNackType.Ack
}, ct);
```

## Mainframe (UTF-8 e EBCDIC)

Para integrações com parceiros/mainframe que exigem codificação específica no arquivo marcador:

```csharp
builder.Services.AddMftMailboxSftp(options =>
{
	options.AckNackMode = SftpAckNackMode.MarkerFile;
	options.AckMarkerEncodingName = "utf-8"; // marcador em UTF-8
});
```

```csharp
builder.Services.AddMftMailboxSftp(options =>
{
	options.AckNackMode = SftpAckNackMode.MarkerFile;
	options.AckMarkerEncodingName = "ibm037"; // EBCDIC (CP037)
});
```

Se você precisa somente processar arquivo sem retorno de ACK/NACK remoto:

```csharp
builder.Services.AddMftMailboxSftp(options =>
{
	options.AckNackMode = SftpAckNackMode.None;
});
```

## Segurança

- Prefira autenticação por chave privada.
- Defina HostKeyFingerprint para pinning de host key.
- Evite logar payload e segredos.
- Mantenha diretórios segregados por integração.

## Troubleshooting

- Connection timeout: revise host, porta, firewall e timeout por arquivo.
- Falha em autenticação: valide credenciais/chave e permissões no servidor.
- Erro de host key: confirme fingerprint e política de rotação.
- Muitos arquivos no lote: ajuste MaxBatchSize no núcleo MFT.
