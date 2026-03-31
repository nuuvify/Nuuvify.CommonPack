# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [Unreleased]

### ✨ Added

#### Extension Methods Públicos (2025-10-30)
- **✨ ToPagedList e ToPagedListAsync agora são extension methods públicos**
  - Anteriormente: Métodos internos na classe `IQueryableExtensions`
  - Agora: Extension methods públicos acessíveis via namespace `Nuuvify.CommonPack.UnitOfWork`
  - Benefício: Encadeamento fluente completo com `.Filter()`, `.Sort()` e `.Select()`
  - Exemplo de uso:
    ```csharp
    using Nuuvify.CommonPack.UnitOfWork; // ✨ Adicione este namespace!

    var result = await _repository.GetAll()
        .Where(p => p.IsActive)         // Filtros EF Core
        .Filter(filterModel)            // Filtros dinâmicos
        .Sort("A-Name,D-Price")         // Ordenação múltipla
        .Select(p => new ProductDto     // Projeção
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        })
        .ToPagedListAsync(              // ✨ Extension method encadeável!
            pageIndex: 1,
            pageSize: 20
        );
    ```

- **IQueryableExtensions tornada classe pública**
  - Classe mudou de `internal static` para `public static`
  - Adicionada documentação XML completa para todos os métodos
  - Melhoria na visibilidade e usabilidade da API

### ⚠️ Deprecated

- **IIQueryablePageList** - Interface marcada como obsoleta
  - Mensagem: "Use the extension methods ToPagedList and ToPagedListAsync from IQueryableExtensions (Nuuvify.CommonPack.UnitOfWork namespace) instead. This interface will be removed in a future version."
  - Motivo: Extension methods públicos tornam a interface desnecessária
  - Será removida em versão futura (Major version bump)

- **QueryablePageList** - Classe marcada como obsoleta
  - Mensagem: "Use the extension methods ToPagedList and ToPagedListAsync from IQueryableExtensions (Nuuvify.CommonPack.UnitOfWork namespace) instead. This class will be removed in a future version."
  - Motivo: Classe wrapper desnecessária com extension methods públicos
  - Será removida em versão futura (Major version bump)

### 📚 Documentation

- **README.md atualizado** com novos exemplos de extension methods
  - Seção dedicada: "✨ Extension Methods: ToPagedList e ToPagedListAsync"
  - Exemplos de encadeamento fluente completo
  - Comparação "Antes vs Agora" para facilitar migração
  - Guia de migração das classes obsoletas
  - Exemplos práticos em Controllers e Services

### 🔄 Migration Guide

#### Migração das Classes Obsoletas

**Antes (usando classes obsoletas)**:
```csharp
var queryablePageList = new QueryablePageList();
var result = await queryablePageList.ToPagedListAsync(query, pageIndex, pageSize);
```

**Depois (usando extension methods - recomendado)**:
```csharp
using Nuuvify.CommonPack.UnitOfWork; // Adicione este namespace

var result = await query.ToPagedListAsync(pageIndex, pageSize);
```

#### Pipeline Completo

```csharp
// ✅ Nova forma: Encadeamento fluente completo
var pagedResult = await _repository.GetAll()
    .Filter(filter)                     // Filtros dinâmicos
    .Sort("A-Name,D-Price")             // Ordenação
    .Select(p => new ProductDto         // Projeção
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price
    })
    .ToPagedListAsync(1, 20);           // Paginação
```

### ⚙️ Technical Details

- **Namespace**: `Nuuvify.CommonPack.UnitOfWork`
- **Classe**: `IQueryableExtensions` (agora pública)
- **Métodos**:
  - `ToPagedList<T>(this IQueryable<T>, int pageIndex, int pageSize, int indexFrom = 0)`
  - `ToPagedListAsync<T>(this IQueryable<T>, int pageIndex, int pageSize, int indexFrom = 0, CancellationToken cancellationToken = default)`

### Added (Previous Features)
- Implementação completa do padrão Unit of Work para Entity Framework Core 8.0
- Repository Pattern genérico com suporte a todas operações CRUD
- Sistema de filtros dinâmicos com `QueryOperatorAttribute`
- 10 operadores de filtro: `Equals`, `NotEquals`, `GreaterThan`, `GreaterThanOrEqualTo`, `LessThan`, `LessThanOrEqualTo`, `Contains`, `StartsWith`, `EndsWith`, `ContainsWithLikeForList`
- Paginação inteligente com `IPagedList<T>` e metadados completos
- Suporte a ordenação dinâmica com múltiplos critérios
- Projeções para DTOs com `selector` parameter
- Includes para eager loading de relacionamentos
- Extensões para `IQueryable<T>` com método `Filter()`
- Auditoria automática com campos `DataCadastro`, `UsuarioCadastro`, `DataAlteracao`, `UsuarioAlteracao`
- Integração com `AutoHistory` para rastreamento de mudanças
- Suporte a múltiplos bancos de dados (SQL Server, Oracle, DB2, PostgreSQL, MySQL)
- Expression Factory para construção de queries dinâmicas type-safe
- ModelBuilder Extensions para mapeamentos complexos
- DbContext Extensions para configurações avançadas
- Migrations automáticas com versionamento

### Features

#### Query Operators
- **Equals**: Comparação de igualdade exata
- **NotEquals**: Comparação de diferença
- **GreaterThan**: Maior que
- **GreaterThanOrEqualTo**: Maior ou igual
- **LessThan**: Menor que
- **LessThanOrEqualTo**: Menor ou igual
- **Contains**: Busca parcial em strings (case-sensitive configurável)
- **StartsWith**: Verifica se começa com determinado valor
- **EndsWith**: Verifica se termina com determinado valor
- **ContainsWithLikeForList**: Busca global em múltiplos campos com lógica OR

#### Advanced Query Features
- **CaseSensitive**: Controle de sensibilidade a maiúsculas/minúsculas
- **UseOr**: Combina filtros com lógica OR ao invés de AND
- **UseNot**: Inverte lógica do filtro (NOT)
- **FieldName**: Permite filtrar em propriedades navegacionais (ex: `"Category.Name"`)
- **Filtros em relacionamentos**: Suporte a navegação em propriedades aninhadas

#### Repository Methods
- `Add()`, `AddRange()` - Adicionar entidades
- `Update()`, `UpdateRange()` - Atualizar entidades
- `Remove()`, `RemoveRange()`, `RemoveById()` - Remover entidades
- `FindAsync()` - Buscar por ID
- `GetFirstOrDefaultAsync()` - Primeira entidade que atende critério
- `ExistsAsync()` - Verificar existência
- `CountAsync()` - Contar registros
- `GetAll()` - Query base com predicates, includes e tracking
- `GetPagedListAsync()` - Paginação com filtros dinâmicos
- `GetPagedListAsync<TResult>()` - Paginação com projeção para DTOs

#### Pagination Features
- Metadados completos: `PageNumber`, `PageSize`, `TotalCount`, `TotalPages`
- Navegação: `HasPreviousPage`, `HasNextPage`
- Performance otimizada com queries no banco
- Suporte a ordenação dinâmica
- Validação automática de limites de página

### Documentation
- README completo com exemplos práticos
- Documentação de API reference
- Troubleshooting guide
- Exemplos de casos de uso:
  - CRUD completo com transações
  - Busca avançada com filtros múltiplos
  - Relatórios e agregações
  - Paginação com projeções
  - Auditoria automática
- Documentação de Query Operators
- Guia de configuração e setup
- Exemplos de integração com diferentes bancos

### Technical Improvements
- ✅ **Performance**: Queries otimizadas com projeções e paginação no banco
- ✅ **Type Safety**: Expressões LINQ com tipagem forte
- ✅ **Testabilidade**: Interfaces bem definidas para mocking
- ✅ **Extensibilidade**: Fácil adicionar novos operadores de filtro
- ✅ **Manutenibilidade**: Código limpo seguindo SOLID principles
- ✅ **Compatibilidade**: Suporte a .NET 8.0 e EF Core 8.0

### Dependencies
- Microsoft.EntityFrameworkCore 8.0.11
- Microsoft.EntityFrameworkCore.Relational 8.0.11
- Nuuvify.CommonPack.AutoHistory (internal)
- Nuuvify.CommonPack.UnitOfWork.Abstraction (internal)

### Breaking Changes
Nenhuma breaking change nesta versão inicial.

### Migration Guide
Como esta é a versão inicial, não há migrações necessárias.

### Known Issues
- Nenhum issue conhecido no momento

### Roadmap
Funcionalidades planejadas para próximas versões:
- [ ] Suporte a Soft Delete global
- [ ] Cache de queries compiladas
- [ ] Suporte a GraphQL
- [ ] Profiling e métricas de performance
- [ ] Suporte a NoSQL databases
- [ ] Bulk operations otimizadas
- [ ] Audit log detalhado com histórico de campos
- [ ] Suporte a multi-tenancy
- [ ] Query interceptors customizáveis
- [ ] Geração automática de APIs REST

---

## Versionamento

Este projeto segue [Semantic Versioning](https://semver.org/):
- **MAJOR**: Mudanças incompatíveis na API
- **MINOR**: Novas funcionalidades mantendo compatibilidade
- **PATCH**: Correções de bugs mantendo compatibilidade

## Links

- [GitHub Repository](https://github.com/nuuvify/Nuuvify.CommonPack)
- [NuGet Package](https://www.nuget.org/packages/Nuuvify.CommonPack.UnitOfWork)
- [Documentation](https://github.com/nuuvify/Nuuvify.CommonPack/wiki)
- [Issues](https://github.com/nuuvify/Nuuvify.CommonPack/issues)
- [Contributing Guidelines](https://github.com/nuuvify/Nuuvify.CommonPack/blob/main/CONTRIBUTING.md)
