
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_Nuuvify.CommonPack&metric=alert_status)](https://sonarcloud.io/project/overview?id=nuuvify_Nuuvify.CommonPack)

[![NuGet Version](https://img.shields.io/nuget/v/Nuuvify.CommonPack.MftMailbox.Redis.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox.Redis)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Nuuvify.CommonPack.MftMailbox.Redis.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.MftMailbox.Redis)

Integração Redis para Nuuvify.CommonPack.MftMailbox com idempotência distribuída, cache de status compartilhado e auditoria via streams.

## Índice

- [Quando usar](#quando-usar)
- [Dependências](#dependências)
- [Instalação](#instalação)
- [Configuração](#configuração)
- [Exemplos de uso](#exemplos-de-uso)
  - [Configuração básica](#configuração-básica)
  - [Idempotência distribuída](#idempotência-distribuída)
  - [Cache de status](#cache-de-status)
  - [Auditoria via streams](#auditoria-via-streams)
- [Boas práticas](#boas-práticas)
- [Troubleshooting](#troubleshooting)
- [Compatibilidade](#compatibilidade)

## Quando usar

Use este pacote quando precisar:

- **Idempotência em múltiplas instâncias**: Evitar processamento duplicado de arquivos MFT em ambientes distribuídos (Kubernetes, Azure App Service com scale-out).
- **Cache de status compartilhado**: Reduzir latência e carga em servidores SFTP/HTTP compartilhando status entre instâncias.
- **Auditoria centralizada**: Rastrear todas as operações MFT em uma stream Redis consumível por aplicações externas.

Não use este pacote se:

- Você tem uma única instância e não precisa de coordenação distribuída (use a implementação in-memory do pacote core).
- Você não tem Redis disponível ou não pode adicionar dependência de infraestrutura externa.

## Dependências

### Pacotes Nuuvify.CommonPack

- `Nuuvify.CommonPack.MftMailbox` (1.0.0+): Funcionalidades core do MFT Mailbox.
- `Nuuvify.CommonPack.MftMailbox.Abstraction` (1.0.0+): Contratos e interfaces compartilhados.

### Pacotes externos

- `StackExchange.Redis` (2.8.9+): Cliente Redis com suporte a operações atômicas e streams.
- `Microsoft.Extensions.DependencyInjection.Abstractions` (.NET 8.0+)
- `Microsoft.Extensions.Options` (.NET 8.0+)
- `Microsoft.Extensions.Logging.Abstractions` (.NET 8.0+)

## Instalação

```bash
dotnet add package Nuuvify.CommonPack.MftMailbox.Redis
```

Ou via `PackageReference` no `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Nuuvify.CommonPack.MftMailbox.Redis" Version="1.0.0" />
</ItemGroup>
```

> **Importante**: Este pacote requer que `IConnectionMultiplexer` esteja registrado no container de DI antes de chamar `AddMftMailboxRedis`.

## Configuração

### Pré-requisito: Registrar Redis

```csharp
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

// Opção 1: Registrar IConnectionMultiplexer explicitamente (recomendado para este pacote)
services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse("localhost:6379");
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});

// Opção 2: Se usar AddStackExchangeRedisCache, registre também o IConnectionMultiplexer
services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "MftMailbox:";
});

services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect("localhost:6379"));
```

### Registrar MftMailbox com Redis

```csharp
// 1. Registrar core MftMailbox
services.AddMftMailboxCore(options =>
{
    options.MaxBatchSize = 100;
    options.Retry.MaxRetries = 5;
    options.Retry.InitialDelay = TimeSpan.FromSeconds(2);
});

// 2. Registrar Redis (substitui in-memory)
services.AddMftMailboxRedis(options =>
{
    // Idempotência
    options.IdempotencyKeyPrefix = "mft:idempotency:";
    options.IdempotencyTtl = TimeSpan.FromHours(24);

    // Cache de status
    options.StatusCacheKeyPrefix = "mft:status:";
    options.StatusCacheTtl = TimeSpan.FromMinutes(5);

    // Auditoria
    options.EnableAuditStream = true;
    options.AuditStreamName = "mft-audit";
    options.AuditStreamMaxLength = 100000;
    options.AuditStreamMaxAge = TimeSpan.FromDays(7);
});

// 3. Registrar protocolo específico (SFTP/HTTP)
services.AddMftMailboxSftp(/* ... */);
```

### Configuração via `appsettings.json`

```json
{
  "RedisMftMailbox": {
    "IdempotencyKeyPrefix": "mft:idempotency:",
    "IdempotencyTtl": "24:00:00",
    "StatusCacheKeyPrefix": "mft:status:",
    "StatusCacheTtl": "00:05:00",
    "EnableAuditStream": true,
    "AuditStreamName": "mft-audit",
    "AuditStreamMaxLength": 100000,
    "AuditStreamMaxAge": "7.00:00:00"
  }
}
```

Bind manualmente no `Startup`:

```csharp
services.Configure<RedisMftMailboxOptions>(
    Configuration.GetSection("RedisMftMailbox"));
```

## Exemplos de uso

### Configuração básica

Setup mínimo com valores padrão:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Redis connection
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
    ConnectionMultiplexer.Connect("localhost:6379"));

// MftMailbox core
builder.Services.AddMftMailboxCore();

// Redis integration (defaults)
builder.Services.AddMftMailboxRedis();

// SFTP protocol
builder.Services.AddMftMailboxSftp(options =>
{
    options.Host = "sftp.example.com";
    options.Port = 22;
    options.Username = "user";
    options.PrivateKeyPath = "/path/to/key";
});

var app = builder.Build();
await app.RunAsync();
```

### Idempotência distribuída

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;

public class FileProcessingService
{
    private readonly IMftIdempotencyStore _idempotency;
    private readonly ILogger<FileProcessingService> _logger;

    public FileProcessingService(
        IMftIdempotencyStore idempotency,
        ILogger<FileProcessingService> logger)
    {
        _idempotency = idempotency;
        _logger = logger;
    }

    public async Task ProcessFileAsync(string integrationKey, string fileName)
    {
        var idempotencyKey = $"{integrationKey}|{fileName}";

        // Tenta iniciar processamento (atomic SET NX)
        var canStart = await _idempotency.TryStartAsync(idempotencyKey);
        if (!canStart)
        {
            _logger.LogWarning(
                "Arquivo {FileName} já processado ou em andamento. Pulando.",
                fileName);
            return;
        }

        try
        {
            // Processar arquivo
            await ProcessFileInternalAsync(fileName);

            // Marcar como completo
            await _idempotency.MarkCompletedAsync(idempotencyKey);
            _logger.LogInformation("Arquivo {FileName} processado com sucesso.", fileName);
        }
        catch (Exception ex)
        {
            // Marcar como falho
            await _idempotency.MarkFailedAsync(idempotencyKey, ex.Message);
            _logger.LogError(ex, "Erro ao processar {FileName}.", fileName);
            throw;
        }
    }

    private async Task ProcessFileInternalAsync(string fileName)
    {
        // Lógica de processamento
        await Task.Delay(1000);
    }
}
```

### Cache de status

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

public class TransferMonitorService
{
    private readonly IMftStatusClient _statusClient;
    private readonly ILogger<TransferMonitorService> _logger;

    public TransferMonitorService(
        IMftStatusClient statusClient,
        ILogger<TransferMonitorService> logger)
    {
        _statusClient = statusClient;
        _logger = logger;
    }

    public async Task<TransferStatus?> GetTransferStatusAsync(
        string integrationKey,
        string itemId)
    {
        // Primeira chamada: Redis miss, consulta servidor SFTP/HTTP
        var status = await _statusClient.GetStatusAsync(integrationKey, itemId);

        if (status is null)
        {
            _logger.LogInformation("Transferência {ItemId} não encontrada.", itemId);
            return null;
        }

        _logger.LogInformation(
            "Status da transferência {ItemId}: {State}",
            itemId,
            status.State);

        // Segunda chamada (dentro de 5min): Redis hit, sem consulta ao servidor
        var cachedStatus = await _statusClient.GetStatusAsync(integrationKey, itemId);

        return cachedStatus;
    }
}
```

### Auditoria via streams

```csharp
using Nuuvify.CommonPack.MftMailbox.Abstraction.Interfaces;
using Nuuvify.CommonPack.MftMailbox.Abstraction.Models;

public class FileTransferService
{
    private readonly ITransferAuditSink _auditSink;
    private readonly ILogger<FileTransferService> _logger;

    public FileTransferService(
        ITransferAuditSink auditSink,
        ILogger<FileTransferService> logger)
    {
        _auditSink = auditSink;
        _logger = logger;
    }

    public async Task TransferFileAsync(string fileName)
    {
        var entry = new TransferAuditEntry
        {
            IntegrationKey = "integration-1",
            ItemId = fileName,
            Direction = TransferDirection.Send,
            State = TransferState.InProgress,
            Timestamp = DateTimeOffset.UtcNow,
            User = Environment.UserName,
            Host = Environment.MachineName
        };

        // Fire-and-forget: não bloqueia se Redis falhar
        await _auditSink.WriteAsync(entry);

        _logger.LogInformation("Auditoria registrada para {FileName}.", fileName);
    }
}
```

**Consumindo a stream Redis** (aplicação externa):

```bash
# CLI Redis
redis-cli XREAD COUNT 10 STREAMS mft-audit 0

# Saída exemplo:
# 1) "mft-audit"
# 2) 1) 1) "1234567890123-0"
#       2) 1) "IntegrationKey"
#          2) "integration-1"
#          3) "ItemId"
#          4) "file.txt"
#          5) "Direction"
#          6) "Send"
#          7) "State"
#          8) "InProgress"
```

## Cenários de uso

Este pacote é projetado para três padrões principais. Escolha o método apropriado baseado em seu cenário:

### Cenário 1: Processar um arquivo recebido (evitar duplicação)

**Use `ProcessFileAsync(string integrationKey, string fileName)` quando:**

- Um arquivo é recebido em uma caixa de entrada (Mailbox)
- Você precisa processar uma única vez em múltiplas instâncias (Kubernetes, scale-out)
- Riscos: sem idempotência, a mesma transferência poderia ser processada N vezes em paralelo

**Exemplo prático - Receptor SFTP distribuído**:
```csharp
// Polling de caixa de entrada
while (true)
{
    var files = await _sftpClient.ListFilesAsync("/inbox");

    foreach (var file in files)
    {
        // Garante processamento único mesmo com 10 instâncias rodando
        await _fileProcessor.ProcessFileAsync("integration-sftp", file.Name);
    }

    await Task.Delay(TimeSpan.FromSeconds(30));
}
```

**Benefícios**:
- ✅ Idempotência: Arquivo processado uma única vez
- ✅ Distribuído: Funciona em clusters (Kubernetes, App Service scale-out)
- ✅ TTL automático: Entradas expiram após 24h (configurable)

---

### Cenário 2: Consultar status de uma transferência (cache compartilhado)

**Use `GetTransferStatusAsync(string integrationKey, string itemId)` quando:**

- Você precisa saber o estado de um arquivo (Em Progresso, Completo, Falhou, etc.)
- Múltiplas instâncias consultam o mesmo status frequentemente
- Você quer reduzir latência e carga em servidores remotos (SFTP/HTTP)

**Exemplo prático - Dashboard de monitoramento distribuído**:
```csharp
// API que retorna status para múltiplos usuários
public async Task<TransferStatusDto> GetStatusAsync(string integrationKey, string itemId)
{
    // Primeira chamada: Redis miss → consulta servidor (ex: 500ms)
    // Chamadas subsequentes (5min): Redis hit → resposta imediata
    var status = await _statusClient.GetStatusAsync(integrationKey, itemId);

    if (status is null)
        return new TransferStatusDto { State = "NotFound" };

    return new TransferStatusDto
    {
        State = status.State.ToString(),
        Progress = status.Progress,
        CachedAt = DateTimeOffset.UtcNow
    };
}
```

**Benefícios**:
- ✅ Cache compartilhado: Reduz consultas ao servidor SFTP/HTTP
- ✅ Latência: 5-10ms (Redis) vs. 500ms+ (servidor remoto)
- ✅ Fallback: Se Redis cair, consulta diretamente (sem interrupção)

---

### Cenário 3: Registrar operação em auditoria (compliance/rastreabilidade)

**Use `TransferFileAsync(string fileName)` com `_auditSink.WriteAsync(entry)` quando:**

- Você precisa rastrear todas as operações (quem fez o quê, quando)
- Requisitos de compliance/auditoria (LGPD, SOX, PCI-DSS)
- Você quer consumir eventos em tempo real (outras aplicações, webhooks)

**Exemplo prático - Integração SAP com compliance**:
```csharp
public async Task SendFileToSapAsync(string fileName, string content)
{
    try
    {
        // 1. Enviar arquivo para SAP
        await _sapFtpClient.UploadAsync("/inbox", fileName, content);

        // 2. Registrar sucesso em auditoria (fire-and-forget)
        var auditEntry = new TransferAuditEntry
        {
            IntegrationKey = "sap-integration",
            ItemId = fileName,
            State = TransferState.Completed,
            Message = "File sent to SAP successfully"
        };
        await _auditSink.WriteAsync(auditEntry);
    }
    catch (Exception ex)
    {
        // 3. Registrar falha em auditoria
        var auditEntry = new TransferAuditEntry
        {
            IntegrationKey = "sap-integration",
            ItemId = fileName,
            State = TransferState.Failed,
            Message = ex.Message
        };
        await _auditSink.WriteAsync(auditEntry);
        throw;
    }
}
```

**Consumindo eventos em outra aplicação**:
```bash
# Sistema de auditoria lê stream Redis em tempo real
redis-cli XREAD BLOCK 0 STREAMS mft-audit \$

# Cada aplicação pode consumir eventos de forma independente com grupos
redis-cli XGROUP CREATE mft-audit audit-consumer \$ MKSTREAM
redis-cli XREADGROUP GROUP audit-consumer consumer1 STREAMS mft-audit >
```

**Benefícios**:
- ✅ Fire-and-forget: Não bloqueia operação MFT
- ✅ Rastreabilidade: Histórico completo de operações
- ✅ Extensível: Outras apps consomem eventos em tempo real

---

### Tabela de decisão rápida

| Método | Quando usar | Problema que resolve | Exemplo |
|--------|------------|----------------------|---------|
| **ProcessFileAsync** | Processar arquivo 1x | Evitar duplicação em clusters | Polling de caixa recebida |
| **GetTransferStatusAsync** | Consultar status | Reduzir latência e carga | Dashboard de monitoramento |
| **TransferFileAsync + auditoria** | Rastrear operação | Compliance e auditoria | Log de operações para SAP |

---

### Fluxo integrado: Recepcionar, processar e rastrear

```csharp
public class MftIntegrationService
{
    private readonly IMftIdempotencyStore _idempotency;
    private readonly IMftStatusClient _statusClient;
    private readonly ITransferAuditSink _auditSink;
    private readonly ILogger<MftIntegrationService> _logger;

    public MftIntegrationService(
        IMftIdempotencyStore idempotency,
        IMftStatusClient statusClient,
        ITransferAuditSink auditSink,
        ILogger<MftIntegrationService> logger)
    {
        _idempotency = idempotency;
        _statusClient = statusClient;
        _auditSink = auditSink;
        _logger = logger;
    }

    public async Task ProcessIncomingFileAsync(string integrationKey, string fileName)
    {
        var idempotencyKey = $"{integrationKey}|{fileName}";

        // 1️⃣ IDEMPOTÊNCIA: Verificar se já foi processado
        var canStart = await _idempotency.TryStartAsync(idempotencyKey);
        if (!canStart)
        {
            _logger.LogInformation("Arquivo {FileName} já foi processado.", fileName);
            return;
        }

        try
        {
            // 2️⃣ AUDITORIA: Registrar início do processamento
            await _auditSink.WriteAsync(new TransferAuditEntry
            {
                IntegrationKey = integrationKey,
                ItemId = fileName,
                State = TransferState.InProgress,
                Message = "Iniciando processamento"
            });

            // 3️⃣ PROCESSAR: Lógica de negócio
            var result = await ProcessFileInternalAsync(fileName);

            // 4️⃣ STATUS: Atualizar status no cache
            // (Este método é interno; GetTransferStatusAsync o consulta)
            await _statusClient.UpdateStatusAsync(integrationKey, fileName, TransferState.Completed);

            // 5️⃣ MARCAR COMO COMPLETO
            await _idempotency.MarkCompletedAsync(idempotencyKey);

            // 6️⃣ AUDITORIA: Registrar sucesso
            await _auditSink.WriteAsync(new TransferAuditEntry
            {
                IntegrationKey = integrationKey,
                ItemId = fileName,
                State = TransferState.Completed,
                Message = $"Processado com sucesso: {result}"
            });

            _logger.LogInformation("Arquivo {FileName} processado com sucesso.", fileName);
        }
        catch (Exception ex)
        {
            // 7️⃣ MARCAR COMO FALHO
            await _idempotency.MarkFailedAsync(idempotencyKey, ex.Message);

            // 8️⃣ AUDITORIA: Registrar falha
            await _auditSink.WriteAsync(new TransferAuditEntry
            {
                IntegrationKey = integrationKey,
                ItemId = fileName,
                State = TransferState.Failed,
                Message = ex.Message
            });

            _logger.LogError(ex, "Erro ao processar {FileName}.", fileName);
            throw;
        }
    }

    public async Task<TransferStatus?> CheckFileStatusAsync(string integrationKey, string fileName)
    {
        // Usa cache Redis para resposta rápida
        return await _statusClient.GetStatusAsync(integrationKey, fileName);
    }

    private async Task<string> ProcessFileInternalAsync(string fileName)
    {
        // Sua lógica de processamento aqui
        await Task.Delay(1000);
        return $"Processado: {fileName}";
    }
}
```

## Boas práticas

### Idempotência

- **TTL adequado**: Configure `IdempotencyTtl` com base na janela de retry do seu sistema (ex: se retries ocorrem em até 24h, use 24h).
- **Chave única**: Use `{integrationKey}|{itemId}` para evitar colisões entre diferentes integrações.
- **Tratamento de erros**: Idempotência **lança exceção** se Redis falhar (hard stop); certifique-se de que Redis está disponível.

### Cache de status

- **TTL vs. staleness**: Configure `StatusCacheTtl` com base em quão stale você tolera (5min é aceitável para maioria dos cenários MFT).
- **Cache negativo**: Respostas `null` são cacheadas por 1 minuto para evitar consultas repetidas a "não encontrado".
- **Graceful degradation**: Se Redis falhar, o cache **degrada silenciosamente** para o cliente original (sem interrupção).

### Auditoria

- **Fire-and-forget**: Auditoria **não bloqueia** operações MFT; falhas são logadas mas não propagam exceções.
- **Retenção**: Configure `AuditStreamMaxLength` e `AuditStreamMaxAge` com base em volume e requisitos de compliance.
- **Consumo externo**: Use grupos de consumo (`XGROUP`) para múltiplos consumers sem perda de mensagens.

### Segurança

- **Conexão segura**: Use TLS/SSL para conexões Redis em produção:
  ```csharp
  options.Configuration = "redis.example.com:6380,ssl=true,password=YOUR_PASSWORD";
  ```
- **Credenciais seguras**: Não commite senhas no código; use Azure Key Vault, AWS Secrets Manager ou variáveis de ambiente.
- **Network isolation**: Configure Redis em VNET privada ou com firewall rules restritivos.

### Performance

- **Connection pooling**: `IConnectionMultiplexer` é thread-safe; registre como Singleton no DI.
- **Timeout configurável**: Ajuste `RedisTimeoutMs` para cenários de alta latência (padrão: 5000ms).
- **Pipeline batching**: Para operações em lote, use `IBatch` do StackExchange.Redis (não suportado diretamente por este pacote).

## Troubleshooting

### Erro: `IConnectionMultiplexer não está registrado no container de DI`

**Causa**: `AddMftMailboxRedis` foi chamado antes de registrar Redis.

**Solução**: Certifique-se de registrar Redis primeiro:

```csharp
// ❌ Ordem errada
services.AddMftMailboxRedis();  // Erro!
services.AddStackExchangeRedisCache(/* ... */);

// ✅ Ordem correta
services.AddStackExchangeRedisCache(/* ... */);
services.AddMftMailboxRedis();
```

### Erro: `RedisConnectionException: It was not possible to connect to the redis server(s)`

**Causa**: Redis está inacessível ou configuração de conexão incorreta.

**Solução**:

1. Verifique se Redis está rodando: `redis-cli ping` (deve retornar `PONG`).
2. Verifique firewall e network rules.
3. Teste a connection string manualmente:
   ```bash
   redis-cli -h localhost -p 6379 PING
   ```

### Cache sempre retorna `null` mesmo com dados existentes

**Causa**: `StatusCacheKeyPrefix` diferente entre gravação e leitura.

**Solução**: Certifique-se de que o prefixo é consistente em todas as instâncias:

```csharp
// Mesma configuração em todas as instâncias
options.StatusCacheKeyPrefix = "mft:status:";
```

### Stream Redis crescendo indefinidamente

**Causa**: `AuditStreamMaxLength` não está sendo respeitado ou é muito alto.

**Solução**: Ajuste `AuditStreamMaxLength` e `AuditStreamMaxAge`:

```csharp
options.AuditStreamMaxLength = 50000;  // Reduzir limite
options.AuditStreamMaxAge = TimeSpan.FromDays(3);  // Reduzir retenção
```

Ou limpe manualmente:

```bash
redis-cli XTRIM mft-audit MAXLEN ~ 10000
```

### Testes unitários falhando com "Redis connection required"

**Causa**: Testes tentando conectar a Redis real.

**Solução**: Use mocks ou `NullConnectionMultiplexer` para testes:

```csharp
// Teste com mock
var mockConnection = new Mock<IConnectionMultiplexer>();
var mockDatabase = new Mock<IDatabase>();
mockConnection.Setup(x => x.GetDatabase(It.IsAny<int>(), null))
    .Returns(mockDatabase.Object);

var store = new RedisMftIdempotencyStore(
    mockConnection.Object,
    Options.Create(new RedisMftMailboxOptions()),
    logger);
```

## Compatibilidade

- **Framework**: .NET 8.0+
- **Redis**: 5.0+ (recomendado: 7.0+ para melhor suporte a streams)
- **StackExchange.Redis**: 2.8.9+
- **Sistema operacional**: Windows, Linux, macOS
- **Cloud providers**: Azure Cache for Redis, AWS ElastiCache, Google Cloud Memorystore

### Limitações conhecidas

- **Moq compatibility**: Alguns métodos do `StackExchange.Redis` têm limitações conhecidas com Moq devido a parâmetros enum e opcionais. Testes que precisam verificar chamadas exatas a `StringSetAsync` devem usar callbacks em vez de `Verify()`.
- **Redis Cluster**: Totalmente suportado, mas chaves devem usar `{hash tags}` para garantir que operações relacionadas fiquem no mesmo slot.
- **Redis Sentinel**: Suportado via `ConfigurationOptions` do StackExchange.Redis.

---

**Documentação completa**: [Nuuvify.CommonPack](https://github.com/nuuvify/Nuuvify.CommonPack)
**Issues e suporte**: [GitHub Issues](https://github.com/nuuvify/Nuuvify.CommonPack/issues)
**Licença**: MIT
