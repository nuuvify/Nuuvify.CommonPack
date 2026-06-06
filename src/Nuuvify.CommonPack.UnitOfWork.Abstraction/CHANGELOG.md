# Changelog - Nuuvify.CommonPack.UnitOfWork.Abstraction

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado
- Nova interface `IShortLivedDbContextFactory<TContext>` para criação explícita de contexto EF curto por operação com suporte a auditoria (`usernameContext`/`userIdContext`).
- Nova interface `IWorkerDbContextFactory<TContext>` para cenários de worker/background com contrato especializado sobre contexto EF curto.

### Alterado

### Corrigido

### Removido

### Segurança

## [Sem versão registrada] - 2025-10-30

### Removido
- `IIQueryablePageList` marcada como obsoleta — use os extension methods `ToPagedList` e `ToPagedListAsync` do namespace `Nuuvify.CommonPack.UnitOfWork`.
- `QueryablePageList` marcada como obsoleta — use os extension methods `ToPagedList` e `ToPagedListAsync` do namespace `Nuuvify.CommonPack.UnitOfWork`.

## [Sem versão registrada] - 2025-10-08

### Adicionado
- Release inicial do `Nuuvify.CommonPack.UnitOfWork.Abstraction`.
- Interface `IServiceBusMessageSender` com contrato completo para operações de filas e tópicos.
- Interfaces `IPagedList<T>`, `IRepository<T>` e `IUnitOfWork` para Repository Pattern.
- Operadores de filtro dinâmico via `QueryOperatorAttribute`.
- Interface `IIQueryablePageList` (obsoleta desde 2025-10-30).


