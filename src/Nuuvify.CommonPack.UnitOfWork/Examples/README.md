# Examples - Nuuvify.CommonPack.UnitOfWork

Esta pasta contém exemplos práticos e abrangentes demonstrando todas as funcionalidades da biblioteca **Nuuvify.CommonPack.UnitOfWork**.

## 📁 Estrutura dos Exemplos

### [`UnitOfWorkUsageExamples.cs`](./UnitOfWorkUsageExamples.cs)
Exemplos completos de uso do Unit of Work pattern incluindo:
- ✅ Configuração e setup
- ✅ Operações CRUD básicas (Create, Read, Update, Delete)
- ✅ Transações e rollback automático
- ✅ Queries básicas e avançadas
- ✅ Includes e relacionamentos
- ✅ Projeções e mapeamento para DTOs

### [`QueryOperatorExamplesSimplified.cs`](./QueryOperatorExamplesSimplified.cs)
Demonstração prática usando **IRepository&lt;T&gt;** e **IPagedList&lt;T&gt;**:
- ✅ **Paginação Real**: `GetPagedListAsync()` retornando `IPagedList<T>`
- ✅ **CRUD via Repository**: `Add()`, `Update()`, `Remove()`, `FindAsync()`
- ✅ **Queries ReadOnly**: `GetAll()`, `GetFirstOrDefaultAsync()`
- ✅ **Filtros Dinâmicos**: Predicates com múltiplos critérios
- ✅ **Busca Global**: Lógica OR em múltiplos campos
- ✅ **Agregações**: Group by, Sum, Count, Average via Repository

### [`PaginationExamples.cs`](./PaginationExamples.cs)
Casos de uso para paginação inteligente:
- ✅ Paginação básica com metadados
- ✅ Paginação com filtros dinâmicos
- ✅ Ordenação múltipla (`"Field1 asc, Field2 desc"`)
- ✅ Paginação sem filtros (listas completas)
- ✅ Otimizações de performance
- ✅ Validações de limites de página

### [`AdvancedFilterExamples.cs`](./AdvancedFilterExamples.cs)
Cenários avançados e casos complexos:
- ✅ Filtros com combinações lógicas (AND, OR, NOT)
- ✅ Ranges de data e valor
- ✅ Filtros em propriedades navegacionais (relacionamentos)
- ✅ Case-insensitive e internacionalização
- ✅ Filtros nullable e campos opcionais
- ✅ Busca global com múltiplos critérios

### [`PerformanceExamples.cs`](./PerformanceExamples.cs)
Boas práticas para performance e otimização:
- ✅ Projeções eficientes (Select específicos)
- ✅ Includes inteligentes (evitar N+1)
- ✅ Paginação otimizada no banco
- ✅ Compilação de queries reutilizáveis
- ✅ Profiling e monitoramento de queries
- ✅ Cache de resultados

## 🎯 Casos de Uso por Categoria

### 🔍 Buscas e Filtros
- **Busca simples**: Filtros por campo único
- **Busca composta**: Múltiplos filtros combinados
- **Busca global**: Termo único em múltiplos campos
- **Busca facetada**: Filtros categorizados (preço, categoria, etc.)
- **Busca avançada**: Operadores lógicos complexos

### 📊 Paginação e Ordenação
- **Lista paginada**: Para interfaces de usuário
- **Export paginado**: Para relatórios grandes
- **Ordenação dinâmica**: Critérios definidos pelo usuário
- **Ordenação múltipla**: Múltiplos campos e direções

### 🏗️ Padrões Arquiteturais
- **Repository Pattern**: Com Unit of Work
- **Service Layer**: Lógica de negócio centralizada
- **CQRS Simplificado**: Separação de comando e consulta
- **DTO Mapping**: Projeções automáticas

### 🚀 Performance
- **Lazy Loading**: Carregamento sob demanda
- **Eager Loading**: Includes estratégicos
- **Query Compilation**: Cache de expressões
- **Projection**: Redução de dados transferidos

## 🏃‍♂️ Como Executar os Exemplos

### 1. Setup Básico

```csharp
// Program.cs ou Startup.cs
builder.Services.AddDbContext<ExampleDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddUnitOfWork<ExampleDbContext>();
```

### 2. Modelo de Dados

Todos os exemplos usam os mesmos modelos consistentes:
- `Product` - Produtos com categorias, preços, estoque
- `Order` - Pedidos com cliente, itens, status
- `Customer` - Clientes com dados pessoais
- `Category` - Categorias hierárquicas

### 3. Executando Exemplos

```csharp
// Injeção de dependência em controller/service
public class ProductController : ControllerBase
{
    private readonly IUnitOfWork<ExampleDbContext> _unitOfWork;
    
    public ProductController(IUnitOfWork<ExampleDbContext> unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    // Use os exemplos diretamente aqui
}
```

## 📋 Checklist de Funcionalidades Demonstradas

### ✅ Unit of Work Pattern
- [x] Configuração e registro no DI
- [x] Operações CRUD básicas
- [x] Transações automáticas
- [x] Rollback em caso de erro
- [x] Queries com Include
- [x] Projeções para DTOs

### ✅ Filtros Dinâmicos (13 Operadores)
- [x] **Equals** - Igualdade exata
- [x] **NotEquals** - Diferente de
- [x] **GreaterThan** - Maior que
- [x] **LessThan** - Menor que
- [x] **GreaterThanOrEqualTo** - Maior ou igual
- [x] **LessThanOrEqualTo** - Menor ou igual
- [x] **Contains** - Contém texto
- [x] **StartsWith** - Inicia com texto
- [x] **GreaterThanOrEqualWhenNullable** - Maior/igual nullable
- [x] **LessThanOrEqualWhenNullable** - Menor/igual nullable
- [x] **EqualsWhenNullable** - Igualdade nullable
- [x] **🆕 ContainsWithLikeForList** - Busca OR múltipla

### ✅ Modificadores de Comportamento
- [x] **CaseSensitive** - Controle de maiúsculas/minúsculas
- [x] **UseOr** - Lógica OR ao invés de AND
- [x] **UseNot** - Negação de condições

### ✅ Paginação e Ordenação
- [x] Paginação básica (PageIndex, PageSize)
- [x] Metadados de paginação (Total, HasNext, etc.)
- [x] Ordenação simples (`"Name asc"`)
- [x] Ordenação múltipla (`"Category asc, Price desc"`)
- [x] Validação de limites

### ✅ Performance e Otimização
- [x] Queries eficientes com projeção
- [x] Includes inteligentes
- [x] Paginação no banco de dados
- [x] Validações de entrada
- [x] Tratamento de erros

## 🔗 Links Úteis

- [📖 README Principal](../README.md) - Documentação completa
- [📋 CHANGELOG](../CHANGELOG.md) - Histórico de mudanças  
- [🧪 Testes](../../../test/Nuuvify.CommonPack.UnitOfWork.InMemory.xTest/) - Testes automatizados
- [📦 NuGet Package](https://www.nuget.org/packages/Nuuvify.CommonPack.UnitOfWork/) - Pacote oficial

## 💡 Dicas Importantes

1. **Sempre use paginação** para listas grandes
2. **Projete para DTOs** para melhor performance  
3. **Valide parâmetros** antes de executar queries
4. **Use Include estrategicamente** para evitar N+1
5. **Teste com dados reais** para validar performance
6. **Monitor queries SQL** em ambiente de produção

---

💡 **Dica**: Execute os exemplos em ordem crescente de complexidade para melhor compreensão das funcionalidades.