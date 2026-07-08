# Changelog - Nuuvify.CommonPack.MftMailbox.Redis

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Documentação
- Adicionado `README.md` completo em português brasileiro para publicação no nuget.org.
- Documentação inclui: instalação, configuração, exemplos práticos, boas práticas, troubleshooting e compatibilidade.
- Exemplos de código funcionais alinhados aos contratos públicos atuais.

## [1.0.0] - 2025-01-13

### Adicionado
- Release inicial do pacote de integração Redis para MFT Mailbox.
- **RedisMftIdempotencyStore**: Armazenamento de idempotência distribuído usando operações atômicas SET NX EX para prevenir duplicação de arquivos em ambientes multi-instância.
- **RedisCachedStatusClient**: Camada de cache decorator pattern para consultas de status com TTL de 5 minutos e fallback gracioso ao cliente original em falhas do Redis.
- **RedisAuditStreamSink**: Sink de auditoria fire-and-forget usando Redis Streams (XADD) para consumidores de telemetria externos.
- **RedisMftMailboxOptions**: Classe de configuração com 9 opções para feature toggles (idempotência, cache de status, stream de auditoria) e tuning de recursos (TTL, timeout, retenção de stream).
- **RedisStatusSerializer**: Serialização JSON para objetos TransferStatus no cache.
- **RedisAuditSerializer**: Serialização de entradas de stream (formato XADD) para registros de auditoria.
- **RedisMftMailboxSetup**: Método de extensão para orquestração de dependency injection com validação de pré-requisitos.
- Cobertura completa de testes unitários (17 casos de teste) com isolamento baseado em mocks.
- Documentação XML completa em todos os tipos e membros públicos.

### Dependências
- StackExchange.Redis: Connection pooling distribuído e operações atômicas.
- Microsoft.Extensions.Options: Binding e validação de configuração.
- Microsoft.Extensions.DependencyInjection.Abstractions: Registro de serviços.
- Microsoft.Extensions.Logging: Logging de diagnóstico (DEBUG/ERROR/WARNING).

### Princípios de Design
- **Atomicidade**: SET NX EX previne processamento duplicado em cenários multi-instância.
- **Resiliência**: Fallback de cache e auditoria não-bloqueante garantem que indisponibilidade do Redis não interrompe transferências.
- **Configuração Orientada**: Feature toggles permitem habilitação seletiva de idempotência, caching e auditoria sem mudanças de código.
- **Observabilidade**: Logging estruturado em pontos-chave de decisão (cache hit/miss, decisão de idempotência, drop de auditoria).
- **Clean Architecture**: Serviços implementam interfaces de domínio (IMftIdempotencyStore, IMftStatusClient, ITransferAuditSink).

### Restrições e Notas
- **Pré-requisito**: IConnectionMultiplexer deve estar previamente registrado no DI (AddStackExchangeRedisCache ou equivalente).
- **TTL de Idempotência**: Padrão de 24 horas garante compliance com janelas de retry enquanto limpa entradas antigas automaticamente.
- **Cache de Status**: TTL de 5 minutos aceitável para maioria dos cenários MFT; configurável por deployment.
- **Retenção de Auditoria**: Padrão de 100k entradas balanceia completude de trilha de auditoria vs. uso de memória do Redis.
- **Semântica de Erros**: Idempotência lança em falha do Redis (hard stop); auditoria/cache degradam graciosamente em falha.
