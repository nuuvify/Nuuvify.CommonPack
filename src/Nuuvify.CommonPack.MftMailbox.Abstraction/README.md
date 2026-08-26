
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_Nuuvify.CommonPack&metric=alert_status)](https://sonarcloud.io/project/overview?id=nuuvify_Nuuvify.CommonPack)

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

## ACK/NACK - Confirmação de processamento

### O que é ACK/NACK?

ACK/NACK é um mecanismo de **confirmação de processamento** para transferências de arquivos recebidas. Funciona como um handshake entre cliente e servidor:

- **ACK (Acknowledgment)**: "Recebi e processei o arquivo com sucesso" → remove arquivo da caixa de entrada
- **NACK (Negative Acknowledgment)**: "Recebi, mas houve erro no processamento" → mantém arquivo na caixa para reprocessamento

### Comportamento esperado

```
1. Arquivo chega na caixa de entrada (Mailbox)
   ↓
2. Sua aplicação recebe o arquivo via ReceiveSingleAsync
   ↓
3. Aplicação processa o arquivo (parseia, valida, persiste)
   ↓
4a. SUCESSO: AckOrNackAsync(..., isAck: true)
    └─ Arquivo removido da caixa
    └─ Não será reprocessado
   ↓
4b. ERRO: AckOrNackAsync(..., isAck: false)
    └─ Arquivo mantido na caixa
    └─ Reprocessamento automático na próxima tentativa
    └─ Histórico de tentativas preservado
```

### Exemplo 1: Processamento bem-sucedido (ACK)

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

public class InboundOrderProcessor
{
    private readonly IMftClientFactory _factory;
    private readonly IOrderRepository _orderRepository;
    private readonly ILogger<InboundOrderProcessor> _logger;

    public InboundOrderProcessor(
        IMftClientFactory factory,
        IOrderRepository orderRepository,
        ILogger<InboundOrderProcessor> logger)
    {
        _factory = factory;
        _orderRepository = orderRepository;
        _logger = logger;
    }

    public async Task ProcessOrderFileAsync(string integrationKey, CancellationToken ct)
    {
        var inboundClient = _factory.CreateInboundClient(MftProtocol.Sftp);
        var ackNackClient = _factory.CreateAckNackClient(MftProtocol.Sftp);

        // 1. Receber arquivo
        var envelope = new TransferEnvelope
        {
            IntegrationKey = integrationKey,
            Protocol = MftProtocol.Sftp
        };

        var result = await inboundClient.ReceiveSingleAsync(envelope, ct);

        if (result.Item is null)
        {
            _logger.LogInformation("Nenhum arquivo disponível na caixa de entrada.");
            return;
        }

        try
        {
            // 2. Processar arquivo
            using var stream = await result.Item.ContentFactory(ct);
            var orders = await ParseOrdersAsync(stream, ct);

            // 3. Persistir no banco
            await _orderRepository.InsertAsync(orders, ct);

            _logger.LogInformation(
                "Arquivo {FileName} processado: {Count} pedidos importados",
                result.Item.FileName,
                orders.Count);

            // ✅ 4. ACK: Arquivo processado com sucesso
            var ackCommand = new AckNackCommand
            {
                IntegrationKey = integrationKey,
                ItemId = result.Item.ItemId,
                FileName = result.Item.FileName,
                IsAck = true,
                Message = $"Importados {orders.Count} pedidos com sucesso"
            };

            await ackNackClient.AckOrNackAsync(ackCommand, ct);
            _logger.LogInformation("ACK enviado para {FileName}", result.Item.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar {FileName}", result.Item?.FileName);

            // ❌ Se houver erro: NACK para reprocessamento
            if (result.Item is not null)
            {
                var nackCommand = new AckNackCommand
                {
                    IntegrationKey = integrationKey,
                    ItemId = result.Item.ItemId,
                    FileName = result.Item.FileName,
                    IsAck = false,
                    Message = $"Erro: {ex.Message}"
                };

                await ackNackClient.AckOrNackAsync(nackCommand, ct);
                _logger.LogWarning("NACK enviado para {FileName} (será reprocessado)", result.Item.FileName);
            }

            throw;
        }
    }

    private async Task<List<Order>> ParseOrdersAsync(Stream stream, CancellationToken ct)
    {
        // Lógica de parsing do CSV/XML
        var orders = new List<Order>();
        // ... parsing logic ...
        return orders;
    }
}
```

### Exemplo 2: Processamento com erro validação (NACK)

```csharp
public class OrderValidationProcessor
{
    private readonly IMftClientFactory _factory;
    private readonly IOrderValidator _validator;
    private readonly ILogger<OrderValidationProcessor> _logger;

    public async Task ProcessWithValidationAsync(string integrationKey, CancellationToken ct)
    {
        var inboundClient = _factory.CreateInboundClient(MftProtocol.Sftp);
        var ackNackClient = _factory.CreateAckNackClient(MftProtocol.Sftp);

        var envelope = new TransferEnvelope
        {
            IntegrationKey = integrationKey,
            Protocol = MftProtocol.Sftp
        };

        var result = await inboundClient.ReceiveSingleAsync(envelope, ct);

        if (result.Item is null)
            return;

        try
        {
            using var stream = await result.Item.ContentFactory(ct);
            var orders = await ParseOrdersAsync(stream, ct);

            // Validar cada pedido
            var validationErrors = new List<string>();
            foreach (var order in orders)
            {
                var validationResult = await _validator.ValidateAsync(order, ct);
                if (!validationResult.IsValid)
                {
                    validationErrors.AddRange(validationResult.Errors.Select(e => e.Message));
                }
            }

            if (validationErrors.Any())
            {
                // ❌ NACK: Validação falhou, arquivo será reprocessado
                var nackCommand = new AckNackCommand
                {
                    IntegrationKey = integrationKey,
                    ItemId = result.Item.ItemId,
                    FileName = result.Item.FileName,
                    IsAck = false,
                    Message = $"Validação falhou: {string.Join("; ", validationErrors)}"
                };

                await ackNackClient.AckOrNackAsync(nackCommand, ct);

                _logger.LogWarning(
                    "NACK enviado para {FileName} - Erros de validação",
                    result.Item.FileName);

                return;
            }

            // ✅ ACK: Validação passou
            var ackCommand = new AckNackCommand
            {
                IntegrationKey = integrationKey,
                ItemId = result.Item.ItemId,
                FileName = result.Item.FileName,
                IsAck = true,
                Message = $"Validado com sucesso: {orders.Count} pedidos"
            };

            await ackNackClient.AckOrNackAsync(ackCommand, ct);
            _logger.LogInformation("ACK enviado para {FileName}", result.Item.FileName);
        }
        catch (Exception ex)
        {
            // ❌ NACK em caso de exceção
            var nackCommand = new AckNackCommand
            {
                IntegrationKey = integrationKey,
                ItemId = result.Item.ItemId,
                FileName = result.Item.FileName,
                IsAck = false,
                Message = ex.Message
            };

            await ackNackClient.AckOrNackAsync(nackCommand, ct);
            _logger.LogError(ex, "Erro crítico - NACK enviado para {FileName}", result.Item.FileName);

            throw;
        }
    }

    private async Task<List<Order>> ParseOrdersAsync(Stream stream, CancellationToken ct)
    {
        var orders = new List<Order>();
        // ... parsing logic ...
        return orders;
    }
}
```

### Exemplo 3: Fluxo com retry automático (Worker pattern)

```csharp
public class MftInboundWorker : BackgroundService
{
    private readonly IMftClientFactory _factory;
    private readonly IOrderProcessor _processor;
    private readonly ILogger<MftInboundWorker> _logger;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var inboundClient = _factory.CreateInboundClient(MftProtocol.Sftp);
        var ackNackClient = _factory.CreateAckNackClient(MftProtocol.Sftp);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Polling da caixa de entrada
                var envelope = new TransferEnvelope
                {
                    IntegrationKey = "sap-orders",
                    Protocol = MftProtocol.Sftp
                };

                var result = await inboundClient.ReceiveSingleAsync(envelope, ct);

                if (result.Item is null)
                {
                    _logger.LogDebug("Nenhum arquivo disponível. Próxima tentativa em 30s");
                    await Task.Delay(TimeSpan.FromSeconds(30), ct);
                    continue;
                }

                _logger.LogInformation(
                    "Arquivo recebido: {FileName} ({ItemId})",
                    result.Item.FileName,
                    result.Item.ItemId);

                // Tentar processar com retry
                var success = await ProcessWithRetryAsync(
                    result.Item,
                    envelope.IntegrationKey,
                    ackNackClient,
                    ct);

                if (success)
                {
                    _logger.LogInformation(
                        "Arquivo {FileName} processado com sucesso",
                        result.Item.FileName);
                }
                else
                {
                    _logger.LogWarning(
                        "Arquivo {FileName} não processado - será reprocessado na próxima tentativa",
                        result.Item.FileName);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Worker cancelado");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no worker");
                await Task.Delay(TimeSpan.FromSeconds(60), ct);
            }
        }
    }

    private async Task<bool> ProcessWithRetryAsync(
        InboundTransferItem item,
        string integrationKey,
        IAckNackClient ackNackClient,
        CancellationToken ct)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                // Processar arquivo
                using var stream = await item.ContentFactory(ct);
                var result = await _processor.ProcessAsync(stream, ct);

                // ✅ Sucesso: ACK
                var ackCommand = new AckNackCommand
                {
                    IntegrationKey = integrationKey,
                    ItemId = item.ItemId,
                    FileName = item.FileName,
                    IsAck = true,
                    Message = $"Processado na tentativa {retryCount + 1}"
                };

                await ackNackClient.AckOrNackAsync(ackCommand, ct);
                return true;
            }
            catch (Exception ex)
            {
                retryCount++;

                _logger.LogWarning(
                    ex,
                    "Erro ao processar {FileName} (tentativa {Retry}/{Max})",
                    item.FileName,
                    retryCount,
                    maxRetries);

                if (retryCount >= maxRetries)
                {
                    // ❌ Falha após retries: NACK
                    var nackCommand = new AckNackCommand
                    {
                        IntegrationKey = integrationKey,
                        ItemId = item.ItemId,
                        FileName = item.FileName,
                        IsAck = false,
                        Message = $"Falha após {maxRetries} tentativas: {ex.Message}"
                    };

                    await ackNackClient.AckOrNackAsync(nackCommand, ct);
                    return false;
                }

                // Aguardar antes de retry
                await Task.Delay(TimeSpan.FromSeconds(5 * retryCount), ct);
            }
        }

        return false;
    }
}
```

### Tabela de decisão: ACK vs NACK

| Situação | Ação | Comportamento esperado |
|----------|------|------------------------|
| Arquivo parseado e persistido com sucesso | **ACK** | Arquivo removido da caixa |
| Validação falhou (dados inválidos) | **NACK** | Arquivo mantido para reprocessamento |
| Banco de dados indisponível | **NACK** | Retry automático na próxima execução |
| Encoding/formato inválido | **NACK** | Arquivo fica em "quarentena" aguardando correção |
| Processamento bem-sucedido mas com warnings | **ACK** | Arquivo removido (warnings são logados) |

### Boas práticas com ACK/NACK

- ✅ **Sempre enviar ACK/NACK**: Não deixe arquivo "órfão" na caixa
- ✅ **ACK apenas após persistência**: Confirme sucesso após salvar no banco
- ✅ **NACK para erros recuperáveis**: Validação, timeout, recursos temporariamente indisponíveis
- ✅ **Logging detalhado**: Use `Message` no `AckNackCommand` para rastrear motivo
- ✅ **Timeout razoável**: Configure timeout de recebimento compatível com tamanho de arquivo
- ❌ **Não fazer ACK antes de processar**: Se a aplicação falhar, arquivo é perdido
- ❌ **Ignorar NACK**: Sempre aguarde confirmação do servidor

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
