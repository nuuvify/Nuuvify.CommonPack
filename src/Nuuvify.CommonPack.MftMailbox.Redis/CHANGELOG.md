# Changelog - Nuuvify.CommonPack.MftMailbox.Redis

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado

### Alterado

### Corrigido

### Removido

### Segurança

## [1.0.0] - 2025-01-13

### Adicionado
- Release inicial do pacote de integração Redis para MFT Mailbox.
- RedisMftIdempotencyStore para idempotência distribuída via SET NX EX.
- RedisCachedStatusClient para cache de status com TTL configurável e fallback para cliente interno.
- RedisAuditStreamSink para escrita de auditoria em Redis Streams.
- RedisMftMailboxSetup para registro de serviços no container de DI com validação de pré-requisitos.

### Alterado

### Corrigido

### Removido

### Segurança
