# Changelog - Nuuvify.CommonPack.MftMailbox.Redis

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-01-13

### Added
- Initial release of Redis integration package for MFT Mailbox.
- **RedisMftIdempotencyStore**: Distributed idempotency store using atomic SET NX EX operations to prevent file duplication across multi-instance deployments.
- **RedisCachedStatusClient**: Decorator pattern cache layer for status queries with 5-minute TTL and graceful fallback to inner client on Redis failures.
- **RedisAuditStreamSink**: Fire-and-forget audit stream sink using Redis Streams (XADD) for external telemetry consumers.
- **RedisMftMailboxOptions**: Configuration class with 9 options for feature toggles (idempotency, status cache, audit stream) and resource tuning (TTL, timeout, stream retention).
- **RedisStatusSerializer**: JSON serialization for TransferStatus objects in cache.
- **RedisAuditSerializer**: Stream entry serialization (XADD format) for audit records.
- **RedisMftMailboxSetup**: Dependency injection orchestration extension method with prerequisite validation.
- Comprehensive unit test coverage (17 test cases) with full mock-based isolation.
- Complete XML documentation on all public types and members.

### Dependencies
- StackExchange.Redis: Distributed connection pooling and atomic operations.
- Microsoft.Extensions.Options: Configuration binding and validation.
- Microsoft.Extensions.DependencyInjection.Abstractions: Service registration.
- Microsoft.Extensions.Logging: Diagnostic logging (DEBUG/ERROR/WARNING).

### Design Principles
- **Atomicity**: SET NX EX prevents double-processing in multi-instance scenarios.
- **Resilience**: Cache fallback and non-blocking audit ensure Redis unavailability does not interrupt transfers.
- **Configuration-Driven**: Feature toggles allow selective enablement of idempotency, caching, and audit without code changes.
- **Observability**: Structured logging at key decision points (cache hit/miss, idempotency decision, audit drop).
- **Clean Architecture**: Services implement domain interfaces (IMftIdempotencyStore, IMftStatusClient, ITransferAuditSink).

### Constraints & Notes
- **Prerequisite**: IConnectionMultiplexer must already be registered in DI (AddStackExchangeRedisCache or equivalent).
- **Idempotency TTL**: 24-hour default ensures compliance with retry windows while auto-cleaning old entries.
- **Status Cache**: 5-minute TTL acceptable for most MFT scenarios; configurable per deployment.
- **Audit Retention**: 100k stream entries default balances audit trail completeness vs. Redis memory usage.
- **Error Semantics**: Idempotency throws on Redis failure (hard stop); audit/cache gracefully degrade on failure.
