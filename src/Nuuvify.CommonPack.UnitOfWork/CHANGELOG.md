# Changelog - Nuuvify.CommonPack.UnitOfWork

Todas as mudanças notáveis deste pacote serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-br/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/spec/v2.0.0.html).

## [Não Lançado]

### Adicionado
- Nova implementação `ShortLivedDbContextFactory<TContext>` para criação de `DbContext` curto por operação, com aplicação opcional de auditoria no contexto criado.
- Nova extensão de DI `AddShortLivedDbContextFactory<TContext>()` para registrar a factory genérica de contexto curto.
- Nova implementação `WorkerDbContextFactory<TContext>` para cenários de worker/background e extensão de DI `AddWorkerDbContextFactory<TContext>()`.

### Alterado
- Documentação de migração ampliada para cenários de worker/background com contexto curto por operação.

### Corrigido

### Removido

### Segurança

### ⚠️ Depreciação (Controlada)

- Deprecado o padrão de construtor de repository concreto que recebe simultaneamente `DbContext` e `IDbContextFactory<TContext>`.
- O caminho legado permanece compatível nesta versão para transição gradual.
- A remoção do padrão legado está planejada para major version futura.

### 🧭 Guia de Migração

- Em repository concreto, preferir construtor orientado a `IUnitOfWork<TContext>`.
- Inicializar base com `unitOfWork.DbContext`.
- Para contexto curto por operação, usar factory dedicada e aplicar auditoria (`UsernameContext`/`UserIdContext`) no contexto criado.

## [Sem versão registrada] - 2025-10-30

### Adicionado
- `ToPagedList<T>` e `ToPagedListAsync<T>` tornados extension methods públicos no namespace `Nuuvify.CommonPack.UnitOfWork`, permitindo encadeamento fluente com `.Filter()`, `.Sort()` e `.Select()`.
- `IQueryableExtensions` alterada de `internal static` para `public static`.

### Removido
- `IIQueryablePageList` marcada como obsoleta — substituída pelos extension methods públicos.
- `QueryablePageList` marcada como obsoleta — substituída pelos extension methods públicos.

**Guia de migração:** substitua `new QueryablePageList().ToPagedListAsync(query, pageIndex, pageSize)` por `query.ToPagedListAsync(pageIndex, pageSize)` adicionando `using Nuuvify.CommonPack.UnitOfWork;`.

## [Sem versão registrada] - 2025-10-08

### Adicionado
- Release inicial do `Nuuvify.CommonPack.UnitOfWork`.
- Repository Pattern genérico com suporte a operações CRUD, paginação, filtros dinâmicos e ordenação.
- Paginação via `IPagedList<T>` com metadados completos (`TotalCount`, `TotalPages`, `HasNextPage`, `HasPreviousPage`).
- 10 operadores de filtro dinâmico via `QueryOperatorAttribute`.
- Auditoria automática com campos de data e usuário de cadastro e alteração.
- Suporte a múltiplos bancos de dados (SQL Server, Oracle, DB2, PostgreSQL, MySQL).


