# MFT Mailbox — Análise de Uso de Redis

## Gestao de Status

| Campo | Valor |
|---|---|
| Status | Concluido |
| Criado em | 2026-08-06 |
| Atualizado em | 2026-08-06 |
| Responsavel | Time MFT |
| Ultima revisao | 2026-08-06 |

### Historico de Status

| Data | De -> Para | Motivo |
|---|---|---|
| 2026-08-06 | Em andamento -> Concluido | Analise finalizada com recomendacoes |

## Visão Geral Atual

A implementação MftMailbox possui 3 abstrações-chave com implementações em memória:

| Interface | Implementação Atual | Escopo | TTL |
|-----------|-------------------|--------|-----|
| `IMftIdempotencyStore` | `InMemoryMftIdempotencyStore` | Dicionário concorrente | Vida do processo |
| `ITransferAuditSink` | `NullTransferAuditSink` | Descarta tudo | N/A |
| `IMftStatusClient` | Cache em memória (SFTP) / HTTP (HTTP) | Local ou remoto | Vida do processo (SFTP) |

---

## Cenários onde Redis é Apropriado

### 1. **IMftIdempotencyStore → Redis (CRÍTICO EM PRODUÇÃO)**

#### Problema com `InMemoryMftIdempotencyStore`

```csharp
// Instância 1: Processa itemId=A, marca como started
// Estado em memória: { "integrationKey|itemId|A": "started" }

// Instância 2: Tenta processar itemId=A
// Consulta próprio estado em memória: NÃO encontra
// Resultado: DUPLICAÇÃO DO PROCESSAMENTO
```

#### Solução com Redis

```csharp
// Ambas instâncias consultam mesma chave Redis
public async Task<bool> TryStartAsync(string idempotencyKey, CancellationToken cancellationToken = default)
{
    // Redis SET NX com TTL (ex: 24h)
    var result = await _redis.SetNxWithExpiryAsync(idempotencyKey, "started", TimeSpan.FromHours(24));
    return result;
}
```

**Benefícios:**
- ✅ Garante idempotência entre múltiplas instâncias
- ✅ Durabilidade entre reinicializações
- ✅ TTL automático para limpeza (evita crescimento indefinido)
- ✅ Sem dependência de banco de dados relacional

**Trade-offs:**
- ⚠️ Requer Redis disponível (SPOF se não for HA)
- ⚠️ Latência de rede (ms)
- ⚠️ Problema se Redis for reinicializado: chaves expiram

**Recomendação:** 🔴 **IMPLEMENTAR como opção de produção**

---

### 2. **IMftStatusClient Cache → Redis (AUXILIAR)**

#### Problema Atual

```csharp
// SFTP: Cache em memória, compartilhado entre instâncias? NÃO
// Se instância 1 consulta status, instância 2 não vê resultado

// HTTP: Consulta endpoint remoto sempre
// Se endpoint lento, carga desnecessária
```

#### Solução com Redis (Opcional)

```csharp
public async Task<TransferStatus?> GetStatusAsync(string integrationKey, string itemId, CancellationToken cancellationToken = default)
{
    var cacheKey = $"mft-status:{integrationKey}:{itemId}";

    // Tentar Redis primeiro (shared cache)
    var cached = await _redis.GetAsync<TransferStatus>(cacheKey);
    if (cached != null) return cached;

    // Fallback: consultar fonte (SFTP remoto ou HTTP endpoint)
    var status = await FetchStatusFromSourceAsync(integrationKey, itemId, cancellationToken);

    // Cache por 5 minutos
    if (status != null)
    {
        await _redis.SetAsync(cacheKey, status, TimeSpan.FromMinutes(5));
    }

    return status;
}
```

**Benefícios:**
- ✅ Cache compartilhado entre instâncias
- ✅ Reduz carga em servidor remoto (SFTP/HTTP)
- ✅ Melhora latência de consultas repetidas
- ✅ TTL automático (ex: 5 min)

**Trade-offs:**
- ⚠️ Cache stale (se status mudar durante TTL, demora para refletir)
- ⚠️ Complexidade adicional (fallback, invalidação)

**Recomendação:** 🟡 **IMPLEMENTAR como OPÇÃO (requer flag de configuração)**

---

### 3. **ITransferAuditSink → Redis Streams (TELEMETRIA)**

#### Caso de Uso

```csharp
// Auditoria é fire-and-forget, não deve bloquear transferência
// Redis Streams = buffer assíncrono de mensagens

public async Task WriteAsync(TransferAuditEntry entry, CancellationToken cancellationToken = default)
{
    // Adicionar à stream Redis de forma não-bloqueante
    // Consumidor externo lê e persiste em BD/ElasticSearch/DataLake
    await _redis.StreamAddAsync("mft-audit", entry.ToDictionary(), cancellationToken);
}
```

**Benefícios:**
- ✅ Desacoplamento: escrita não bloqueia transferência
- ✅ Buffer durável enquanto consumidor está fora
- ✅ Suporta múltiplos consumidores (Fan-out)

**Trade-offs:**
- ⚠️ Requer consumidor externo em execução
- ⚠️ Se Redis falhar, auditoria perde registros recentes (não é durável se não houver RDB/AOF)

**Recomendação:** 🟡 **IMPLEMENTAR como ALTERNATIVA a message queue (ex: RabbitMQ)**

---

## Proposta de Arquitetura com Redis

### 1. Criar novo pacote: `Nuuvify.CommonPack.MftMailbox.Redis`

```
src/Nuuvify.CommonPack.MftMailbox.Redis/
├── Configuration/
│   └── RedisMftMailboxOptions.cs        # Conexão, timeouts
├── Services/
│   ├── RedisMftIdempotencyStore.cs       # 🔴 CRÍTICO
│   ├── RedisCachedStatusClient.cs        # 🟡 AUXILIAR
│   └── RedisAuditStreamSink.cs          # 🟡 TELEMETRIA
├── Utilities/
│   └── RedisSerializer.cs               # Serialização de TransferStatus
└── RedisMftMailboxSetup.cs              # Extensão de DI
```

### 2. Padrão de Registro no DI

```csharp
// Uso: com Redis (idempotência obrigatória em produção multi-instância)
services.AddMftMailboxCore();
services.AddMftMailboxRedis(redis =>
{
    redis.ConnectionString = "localhost:6379";
    redis.IdempotencyTtl = TimeSpan.FromHours(24);
    redis.StatusCacheTtl = TimeSpan.FromMinutes(5);    // Opcional
    redis.EnableAuditStream = true;                    // Opcional
});

// Uso: sem Redis (local/teste)
services.AddMftMailboxCore();
```

---

## Implementação Proposta: RedisMftIdempotencyStore

```csharp
namespace Nuuvify.CommonPack.MftMailbox.Redis.Services;

/// <summary>
/// Idempotência distribuída via Redis, com TTL automático.
/// Garante que múltiplas instâncias não processem o mesmo arquivo.
/// </summary>
public sealed class RedisMftIdempotencyStore : IMftIdempotencyStore
{
    private readonly IDatabase _redis;
    private readonly TimeSpan _ttl;
    private readonly ILogger<RedisMftIdempotencyStore> _logger;

    public RedisMftIdempotencyStore(
        IConnectionMultiplexer redis,
        IOptions<RedisMftMailboxOptions> options,
        ILogger<RedisMftIdempotencyStore> logger)
    {
        _redis = redis.GetDatabase();
        _ttl = options.Value.IdempotencyTtl;
        _logger = logger;
    }

    public async Task<bool> TryStartAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // SET NX EX: Set if Not eXists, with EXpiry
            var started = await _redis.StringSetAsync(
                idempotencyKey,
                "started",
                _ttl,
                When.NotExists);

            if (!started)
            {
                _logger.LogDebug("Idempotency key already exists (skipping): {IdempotencyKey}", idempotencyKey);
            }

            return started;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error in TryStartAsync for key {IdempotencyKey}", idempotencyKey);
            throw;
        }
    }

    public async Task MarkCompletedAsync(string idempotencyKey, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            // Atualizar valor, mantendo TTL restante
            var ttl = await _redis.KeyTimeToLiveAsync(idempotencyKey);
            await _redis.StringSetAsync(
                idempotencyKey,
                "completed",
                ttl > TimeSpan.Zero ? ttl : _ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error in MarkCompletedAsync for key {IdempotencyKey}", idempotencyKey);
            throw;
        }
    }

    public async Task MarkFailedAsync(string idempotencyKey, string reason, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            var ttl = await _redis.KeyTimeToLiveAsync(idempotencyKey);
            await _redis.StringSetAsync(
                idempotencyKey,
                $"failed:{reason}",
                ttl > TimeSpan.Zero ? ttl : _ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis error in MarkFailedAsync for key {IdempotencyKey}", idempotencyKey);
            throw;
        }
    }
}
```

---

## Decisões Arquiteturais

| Decisão | Recomendação | Justificativa |
|---------|--------------|---------------|
| **Redis para idempotência** | 🔴 SIM (produção) | Critério obrigatório para multi-instância |
| **Redis para status cache** | 🟡 OPÇÃO (configurável) | Benefício: reduz carga remota; Risco: cache stale |
| **Redis para auditoria** | 🟡 ALTERNATIVA | Usar se não houver Message Queue (RabbitMQ/SB) |
| **Fallback se Redis falhar** | ✅ SIM | Usar `InMemoryMftIdempotencyStore` como fallback |
| **Pool de conexões** | ✅ SIM (Multiplexer único) | IConnectionMultiplexer singleton |
| **Serialização** | ✅ JSON (System.Text.Json) | Padrão do repositório |

---

## Checklist de Implementação

- [ ] Criar pacote `Nuuvify.CommonPack.MftMailbox.Redis`
- [ ] Implementar `RedisMftIdempotencyStore` com logging/retry
- [ ] Implementar `RedisCachedStatusClient` com invalidação
- [ ] Implementar `RedisAuditStreamSink` com múltiplos consumidores
- [ ] Adicionar `RedisMftMailboxSetup` com DI pattern
- [ ] Testes: integrações, failover, TTL expiracy
- [ ] Documentação: exemplo de configuração, troubleshooting
- [ ] Considerar Sentinel/Cluster para HA

---

## Exemplo de Uso Final

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMftMailboxCore(opt =>
{
    opt.MaxBatchSize = 100;
    opt.Retry.MaxRetries = 5;
});

// Em produção: sempre use Redis para idempotência
if (builder.Environment.IsProduction())
{
    builder.Services.AddMftMailboxRedis(redis =>
    {
        redis.ConnectionString = builder.Configuration.GetConnectionString("Redis");
        redis.IdempotencyTtl = TimeSpan.FromHours(24);
        redis.StatusCacheTtl = TimeSpan.FromMinutes(5);
        redis.EnableAuditStream = true;
    });
}

// Protocolo específico
builder.Services.AddMftMailboxSftp(sftp => { /* ... */ });
builder.Services.AddMftMailboxHttp(http => { /* ... */ });

var app = builder.Build();
// ...
```

---

## Prós e Contras Resumidos

### ✅ Prós do Redis
- Distribuído por natureza (multi-instância)
- TTL automático (sem garbage collection manual)
- Operações atômicas (SET NX)
- Baixa latência
- Suporta Streams, Pub/Sub, cache

### ⚠️ Contras
- SPOF se não houver HA/Sentinel
- Dados perdidos se crash sem RDB/AOF
- Complexidade operacional (backup, monitoring)
- Custo adicional de infraestrutura

---

## Conclusão

| Prioridade | Componente | Decisão |
|-----------|-----------|---------|
| 🔴 ALTA | Idempotência distribuída | **Implementar `RedisMftIdempotencyStore` como obrigatório em produção** |
| 🟡 MÉDIA | Status cache compartilhado | Implementar como opcional, com fallback local |
| 🟡 MÉDIA | Auditoria via stream | Avaliar vs. Message Queue existente |

**Próximo Passo:** Implementar `Nuuvify.CommonPack.MftMailbox.Redis` como novo pacote separado.
