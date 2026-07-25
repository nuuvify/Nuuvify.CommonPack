# Changelog - Nuuvify.CommonPack.MftMailbox.Redis

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado
- Store distribuída de idempotência com <c>SET NX EX</c> via Redis.
- Cache de status compartilhado com TTL configurável e fallback para cliente interno.
- Sink de auditoria em Redis Streams com trim assíncrono e observabilidade de falhas.
- Setup de DI com validação de pré-requisitos e decoração opcional de status cache.

### Alterado

### Corrigido

### Removido

### Segurança
