# Changelog

Todas as mudanças notáveis neste projeto serão documentadas neste arquivo.

O formato é baseado em [Keep a Changelog](https://keepachangelog.com/pt-BR/1.0.0/),
e este projeto adere ao [Semantic Versioning](https://semver.org/lang/pt-BR/).

## [Unreleased]

### Added
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
