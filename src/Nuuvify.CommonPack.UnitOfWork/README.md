# Nuuvify.CommonPack.UnitOfWork

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=nuuvify_CommonPack&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=nuuvify_CommonPack)
[![PR Validation](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml/badge.svg)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/pr-validation.yml)
[![Publish and Release](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml/badge.svg?branch=main)](https://github.com/nuuvify/Nuuvify.CommonPack/actions/workflows/publish-release.yml)

Implementação robusta e completa do padrão **Unit of Work** e **Repository** para Entity Framework Core. Fornece uma camada de acesso a dados padronizada com suporte a filtros dinâmicos, paginação inteligente, consultas complexas e auditoria automática.

## 📋 Índice

- [Funcionalidades](#funcionalidades)
- [Instalação](#instalação)
- [Dependências](#dependências)
- [Configuração](#configuração)
- [Uso](#uso)
  - [Unit of Work Básico](#unit-of-work-básico)
  - [Repository Pattern](#repository-pattern)
  - [Filtros Dinâmicos](#filtros-dinâmicos)
  - [Paginação](#paginação)
  - [Query Operators](#query-operators)
  - [Auditoria Automática](#auditoria-automática)
- [Exemplos Práticos](#exemplos-práticos)
- [API Reference](#api-reference)
- [Troubleshooting](#troubleshooting)
- [Changelog](#changelog)

## Funcionalidades

### 🎯 Core Features
- ✅ **Unit of Work Pattern** com gerenciamento automático de transações
- ✅ **Repository Pattern Genérico** para todas as entidades
- ✅ **Filtros Dinâmicos** com Query Operators customizáveis
- ✅ **✨ ToPagedList/ToPagedListAsync** como extension methods públicos encadeáveis
- ✅ **Paginação Inteligente** com metadados completos
- ✅ **Auditoria Automática** de criação e alteração
- ✅ **Suporte a Múltiplos Bancos** (SQL Server, Oracle, DB2, PostgreSQL, MySQL)
- ✅ **Migrations Automáticas** com versionamento

### 🔍 Query Features
- ✅ **Filtros por Atributos** usando `[QueryOperator]`
- ✅ **Operadores Avançados** (Contains, StartsWith, GreaterThan, etc.)
- ✅ **Busca Global** em múltiplos campos simultaneamente
- ✅ **Ordenação Dinâmica** com múltiplos critérios via `.Sort()`
- ✅ **✨ Encadeamento Fluente**: `.Filter()` → `.Sort()` → `.Select()` → `.ToPagedListAsync()`
- ✅ **Projeções** para DTOs com Select otimizado
- ✅ **Includes** para eager loading de relacionamentos

### 🛠️ Advanced Features
- ✅ **Soft Delete** com filtros automáticos
- ✅ **AutoHistory** para rastreamento de mudanças
- ✅ **DbContext Extensions** para configurações avançadas
- ✅ **Model Builder Extensions** para mapeamentos complexos
- ✅ **Expression Factory** para queries dinâmicas type-safe
- ✅ **Compatibilidade .NET 8.0** com recursos modernos

## Instalação

### Via Package Manager Console
```powershell
Install-Package Nuuvify.CommonPack.UnitOfWork
Install-Package Nuuvify.CommonPack.UnitOfWork.Abstraction
```

### Via .NET CLI
```bash
dotnet add package Nuuvify.CommonPack.UnitOfWork
dotnet add package Nuuvify.CommonPack.UnitOfWork.Abstraction
```

### Via PackageReference
```xml
<PackageReference Include="Nuuvify.CommonPack.UnitOfWork" Version="X.X.X" />
<PackageReference Include="Nuuvify.CommonPack.UnitOfWork.Abstraction" Version="X.X.X" />
```

## Dependências

### NuGet Packages

| Package                                       | Version | Descrição                               |
| --------------------------------------------- | ------- | --------------------------------------- |
| **Microsoft.EntityFrameworkCore**             | 8.0.11  | ORM base do Entity Framework Core       |
| **Microsoft.EntityFrameworkCore.Relational**  | 8.0.11  | Suporte para bancos relacionais         |
| **Nuuvify.CommonPack.AutoHistory**            | -       | Rastreamento automático de mudanças     |
| **Nuuvify.CommonPack.UnitOfWork.Abstraction** | -       | Interfaces e abstrações do Unit of Work |

### Database Providers Suportados

```xml
<!-- SQL Server -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.11" />

<!-- Oracle -->
<PackageReference Include="Oracle.EntityFrameworkCore" Version="8.23.50" />

<!-- DB2 -->
<PackageReference Include="IBM.EntityFrameworkCore" Version="8.0.0.400" />

<!-- PostgreSQL -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.10" />

<!-- MySQL -->
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
```

### Framework

- **.NET 8.0**: Framework moderno com performance otimizada e recursos avançados

## Configuração

### 1. Configurar DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações customizadas
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
        });
    }
}
```

### 2. Registrar Serviços (Dependency Injection)

```csharp
// Program.cs (.NET 8)
using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Registrar DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        }
    ));

// Registrar Unit of Work
builder.Services.AddUnitOfWork<AppDbContext>();

// Ou com configurações customizadas
builder.Services.AddScoped<IUnitOfWork, UnitOfWork<AppDbContext>>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

var app = builder.Build();

// Aplicar migrations automaticamente (opcional)
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
}

app.Run();
```

### 3. Configurar AutoHistory (Opcional)

```csharp
using Nuuvify.CommonPack.AutoHistory.Extensions;

public class AppDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Habilitar rastreamento automático de mudanças
        modelBuilder.EnableAutoHistory<AutoHistory>(options =>
        {
            options.TableName = "AutoHistories";
            options.SchemaName = "audit";
        });
    }
}
```

## ✨ Extension Methods: ToPagedList e ToPagedListAsync

**Novidade Importante**: Os métodos `ToPagedList<T>` e `ToPagedListAsync<T>` agora são **extension methods públicos**!

### Como Usar

```csharp
using Nuuvify.CommonPack.UnitOfWork; // ✨ Adicione este namespace para os extension methods
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

// ✅ Encadeamento fluente completo
var pagedResult = await _repository.GetAll()
    .Where(p => p.IsActive)             // Filtros EF Core
    .Filter(filterModel)                // Filtros dinâmicos com [QueryOperator]
    .Sort("A-Name,D-Price")             // Ordenação múltipla
    .Select(p => new ProductDto         // Projeção para DTO
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price
    })
    .ToPagedListAsync(                  // ✨ Extension method público!
        pageIndex: 1,
        pageSize: 20
    );

// Retorna IPagedList<ProductDto> com:
// - Items: Lista de itens da página atual
// - PageIndex, PageSize, TotalCount, TotalPages
// - HasNextPage, HasPreviousPage para navegação
```

### Vantagens do Encadeamento

✅ **API Fluente**: Combina `.Filter()`, `.Sort()` e `.ToPagedListAsync()` naturalmente
✅ **Type-Safe**: IntelliSense completo em cada etapa
✅ **Performance**: Toda query executada no banco (não em memória)
✅ **Flexível**: Funciona com `IQueryable<T>` de qualquer fonte
✅ **Clean Code**: Pipeline de transformação claro e legível

### Comparação: Antes vs Agora

```csharp
// ❌ Forma antiga (ainda funciona, mas verbosa)
var pagedResult = await _repository.GetPagedListAsync(
    predicate: filter,
    selector: p => new ProductDto { Id = p.Id, Name = p.Name },
    orderBy: "Name asc",
    pageIndex: 1,
    pageSize: 20
);

// ✅ Nova forma (recomendada - mais fluente)
var pagedResult = await _repository.GetAll()
    .Filter(filter)
    .Sort("A-Name")
    .Select(p => new ProductDto { Id = p.Id, Name = p.Name })
    .ToPagedListAsync(1, 20);
```

### Classes Obsoletas

⚠️ As seguintes classes estão marcadas como `[Obsolete]`:
- `IIQueryablePageList` - Use extension methods diretamente
- `QueryablePageList` - Use extension methods diretamente

**Migração**:
```csharp
// ❌ Obsoleto
var queryablePageList = new QueryablePageList();
var result = await queryablePageList.ToPagedListAsync(query, 1, 20);

// ✅ Recomendado
using Nuuvify.CommonPack.UnitOfWork;
var result = await query.ToPagedListAsync(1, 20);
```

---

## Uso

### Unit of Work Básico

```csharp
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

public class ProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Product> _productRepository;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _productRepository = unitOfWork.Repository<Product>();
    }

    public async Task<Product> CreateProductAsync(string name, decimal price)
    {
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Price = price,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _productRepository.Add(product);
        await _unitOfWork.SaveChangesAsync();

        return product;
    }

    public async Task<Product?> GetProductByIdAsync(string id)
    {
        return await _productRepository.FindAsync(id);
    }

    public async Task UpdateProductAsync(string id, decimal newPrice)
    {
        var product = await _productRepository.FindAsync(id);
        if (product != null)
        {
            product.Price = newPrice;
            product.UpdatedAt = DateTimeOffset.UtcNow;

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task DeleteProductAsync(string id)
    {
        var product = await _productRepository.FindAsync(id);
        if (product != null)
        {
            _productRepository.Remove(product);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
```

### Repository Pattern

```csharp
public class OrderService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = unitOfWork.Repository<Order>();
        _customerRepository = unitOfWork.Repository<Customer>();
    }

    // CRUD Básico
    public async Task<Order> CreateOrderAsync(CreateOrderCommand command)
    {
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = command.CustomerId,
            OrderDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Pending
        };

        _orderRepository.Add(order);
        await _unitOfWork.SaveChangesAsync();

        return order;
    }

    // Query com Include
    public async Task<Order?> GetOrderWithCustomerAsync(string orderId)
    {
        var orders = _orderRepository.GetAll(
            predicate: o => o.Id == orderId,
            include: query => query.Include(o => o.Customer)
        );

        return await orders.FirstOrDefaultAsync();
    }

    // Query com múltiplos includes
    public async Task<List<Order>> GetOrdersWithDetailsAsync()
    {
        var orders = _orderRepository.GetAll(
            include: query => query
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product)
        );

        return await orders.ToListAsync();
    }

    // Queries assíncronas
    public async Task<bool> OrderExistsAsync(string orderId)
    {
        return await _orderRepository.ExistsAsync(o => o.Id == orderId);
    }

    public async Task<int> GetTotalOrdersCountAsync()
    {
        return await _orderRepository.CountAsync();
    }

    // Bulk operations
    public async Task DeleteMultipleOrdersAsync(List<string> orderIds)
    {
        var orders = await _orderRepository
            .GetAll(predicate: o => orderIds.Contains(o.Id))
            .ToListAsync();

        _orderRepository.RemoveRange(orders);
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### Filtros Dinâmicos

```csharp
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Enums;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

// Modelo de filtro
public class ProductFilterModel : IQueryableCustom
{
    [QueryOperator(WhereOperator.Contains, CaseSensitive = false)]
    public string? Name { get; set; }

    [QueryOperator(WhereOperator.GreaterThanOrEqualTo)]
    public decimal? MinPrice { get; set; }

    [QueryOperator(WhereOperator.LessThanOrEqualTo)]
    public decimal? MaxPrice { get; set; }

    [QueryOperator(WhereOperator.Equals)]
    public bool? InStock { get; set; }

    [QueryOperator(WhereOperator.Contains)]
    public string? Category { get; set; }
}

// Service usando filtros
public class ProductSearchService
{
    private readonly IRepository<Product> _repository;

    public ProductSearchService(IUnitOfWork unitOfWork)
    {
        _repository = unitOfWork.Repository<Product>();
    }

    public async Task<List<Product>> SearchProductsAsync(ProductFilterModel filter)
    {
        var query = _repository.GetAll();

        // Aplicar filtros dinâmicos
        query = query.Filter(filter);

        return await query.ToListAsync();
    }
}
```

### Paginação

```csharp
using Nuuvify.CommonPack.UnitOfWork; // Namespace dos extension methods
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

public class ProductListService
{
    private readonly IRepository<Product> _repository;

    public ProductListService(IUnitOfWork unitOfWork)
    {
        _repository = unitOfWork.Repository<Product>();
    }

    /// <summary>
    /// ✨ Novo: ToPagedListAsync agora é um extension method que pode ser encadeado com Filter() e Sort()
    /// </summary>
    public async Task<IPagedList<Product>> GetProductsPagedAsync(
        ProductFilterModel filter,
        int pageIndex = 1,
        int pageSize = 20)
    {
        // ✅ Encadeamento direto de Filter(), Sort() e ToPagedListAsync()
        var pagedResult = await _repository.GetAll()
            .Filter(filter)              // Filtros dinâmicos
            .Sort("A-Name,D-Price")      // Ordenação múltipla
            .ToPagedListAsync(           // Extension method público!
                pageIndex: pageIndex,
                pageSize: pageSize
            );

        return pagedResult;
    }

    /// <summary>
    /// Paginação com projeção para DTO - encadeamento completo
    /// </summary>
    public async Task<IPagedList<ProductDto>> GetProductsDtoPagedAsync(
        ProductFilterModel filter,
        int pageIndex = 1,
        int pageSize = 20)
    {
        // ✅ Filtros → Ordenação → Projeção → Paginação (tudo encadeado!)
        var pagedResult = await _repository.GetAll()
            .Where(p => p.IsActive)      // Filtro fixo
            .Filter(filter)              // Filtros dinâmicos
            .Sort("A-Category,D-Price")  // Ordenação
            .Select(p => new ProductDto  // Projeção para DTO
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category.Name
            })
            .ToPagedListAsync(           // Paginação como extension method
                pageIndex: pageIndex,
                pageSize: pageSize
            );

        return pagedResult;
    }

    /// <summary>
    /// Exemplo completo: todos os recursos encadeados
    /// </summary>
    public async Task<IPagedList<ProductCardDto>> GetProductCardsAsync(
        string searchTerm,
        string category,
        decimal? minPrice,
        int pageIndex = 1,
        int pageSize = 12)
    {
        var filter = new ProductFilterModel
        {
            Name = searchTerm,
            Category = category,
            MinPrice = minPrice
        };

        // ✅ Pipeline completo: Where → Filter → Sort → Select → ToPagedListAsync
        return await _repository.GetAll()
            .Where(p => p.IsActive && p.Stock > 0)    // Filtros fixos
            .Filter(filter)                           // Filtros dinâmicos
            .Sort("A-Name")                           // Ordenação
            .Select(p => new ProductCardDto           // Projeção
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImageUrl = $"/images/{p.Id}.jpg"
            })
            .ToPagedListAsync(pageIndex, pageSize);   // Paginação
    }
}
```

### Query Operators

```csharp
public class AdvancedFilterModel : IQueryableCustom
{
    // Operador Equals
    [QueryOperator(WhereOperator.Equals)]
    public string? Status { get; set; }

    // Operador NotEquals
    [QueryOperator(WhereOperator.NotEquals)]
    public string? ExcludeCategory { get; set; }

    // Operadores de comparação
    [QueryOperator(WhereOperator.GreaterThan)]
    public decimal? PriceGreaterThan { get; set; }

    [QueryOperator(WhereOperator.LessThan)]
    public decimal? PriceLessThan { get; set; }

    // Operadores de string
    [QueryOperator(WhereOperator.StartsWith, CaseSensitive = false)]
    public string? NameStartsWith { get; set; }

    [QueryOperator(WhereOperator.EndsWith, CaseSensitive = false)]
    public string? NameEndsWith { get; set; }

    [QueryOperator(WhereOperator.Contains, CaseSensitive = false)]
    public string? SearchTerm { get; set; }

    // Operador para listas (busca global)
    [QueryOperator(WhereOperator.ContainsWithLikeForList, CaseSensitive = false)]
    public List<string>? GlobalSearchTerms { get; set; }

    // Operadores com OR
    [QueryOperator(WhereOperator.Contains, UseOr = true)]
    public string? NameOrDescription { get; set; }

    // Operadores com NOT
    [QueryOperator(WhereOperator.Equals, UseNot = true)]
    public string? NotInCategory { get; set; }

    // Operadores de data
    [QueryOperator(WhereOperator.GreaterThanOrEqualTo)]
    public DateTimeOffset? CreatedAfter { get; set; }

    [QueryOperator(WhereOperator.LessThanOrEqualTo)]
    public DateTimeOffset? CreatedBefore { get; set; }

    // Propriedades navegacionais
    [QueryOperator(WhereOperator.Equals, FieldName = "Category.Name")]
    public string? CategoryName { get; set; }

    [QueryOperator(WhereOperator.Contains, FieldName = "Customer.Email")]
    public string? CustomerEmail { get; set; }
}
```

### Auditoria Automática

```csharp
using Nuuvify.CommonPack.Domain;

// Entidade com auditoria
public class Product : DomainEntity
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }

    // Propriedades de auditoria herdadas de DomainEntity:
    // - Id (string)
    // - DataCadastro (DateTimeOffset)
    // - UsuarioCadastro (string)
    // - DataAlteracao (DateTimeOffset?)
    // - UsuarioAlteracao (string)
}

// SaveChanges automático com auditoria
public class AuditService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task CreateProductWithAuditAsync(string name, decimal price)
    {
        var product = new Product
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Price = price
            // DataCadastro e UsuarioCadastro são preenchidos automaticamente
        };

        _unitOfWork.Repository<Product>().Add(product);

        // SaveChanges preenche campos de auditoria automaticamente
        await _unitOfWork.SaveChangesAsync();

        // Propriedades preenchidas:
        // - DataCadastro: Data/hora UTC atual
        // - UsuarioCadastro: Usuário do contexto HTTP (se disponível)
    }

    public async Task UpdateProductWithAuditAsync(string id, decimal newPrice)
    {
        var product = await _unitOfWork.Repository<Product>().FindAsync(id);
        if (product != null)
        {
            product.Price = newPrice;

            _unitOfWork.Repository<Product>().Update(product);
            await _unitOfWork.SaveChangesAsync();

            // DataAlteracao e UsuarioAlteracao são preenchidos automaticamente
        }
    }
}
```

## Exemplos Práticos

### Exemplo 1: CRUD Completo com Transações

```csharp
public class OrderManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<OrderItem> _orderItemRepository;
    private readonly IRepository<Product> _productRepository;

    public OrderManagementService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _orderRepository = unitOfWork.Repository<Order>();
        _orderItemRepository = unitOfWork.Repository<OrderItem>();
        _productRepository = unitOfWork.Repository<Product>();
    }

    public async Task<Order> CreateOrderWithItemsAsync(CreateOrderCommand command)
    {
        // Iniciar transação implícita
        var order = new Order
        {
            Id = Guid.NewGuid().ToString(),
            CustomerId = command.CustomerId,
            OrderDate = DateTimeOffset.UtcNow,
            Status = OrderStatus.Pending
        };

        _orderRepository.Add(order);

        // Adicionar itens do pedido
        foreach (var itemCommand in command.Items)
        {
            var product = await _productRepository.FindAsync(itemCommand.ProductId);
            if (product == null)
            {
                throw new InvalidOperationException($"Produto {itemCommand.ProductId} não encontrado");
            }

            // Verificar estoque
            if (product.Stock < itemCommand.Quantity)
            {
                throw new InvalidOperationException($"Estoque insuficiente para produto {product.Name}");
            }

            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid().ToString(),
                OrderId = order.Id,
                ProductId = product.Id,
                Quantity = itemCommand.Quantity,
                UnitPrice = product.Price,
                TotalPrice = product.Price * itemCommand.Quantity
            };

            _orderItemRepository.Add(orderItem);

            // Atualizar estoque
            product.Stock -= itemCommand.Quantity;
            _productRepository.Update(product);
        }

        // Salvar todas as mudanças em uma única transação
        await _unitOfWork.SaveChangesAsync();

        return order;
    }

    public async Task CancelOrderAsync(string orderId)
    {
        var order = await _orderRepository
            .GetAll(
                predicate: o => o.Id == orderId,
                include: query => query.Include(o => o.Items)
            )
            .FirstOrDefaultAsync();

        if (order == null)
        {
            throw new InvalidOperationException("Pedido não encontrado");
        }

        // Restaurar estoque
        foreach (var item in order.Items)
        {
            var product = await _productRepository.FindAsync(item.ProductId);
            if (product != null)
            {
                product.Stock += item.Quantity;
                _productRepository.Update(product);
            }
        }

        // Atualizar status do pedido
        order.Status = OrderStatus.Cancelled;
        _orderRepository.Update(order);

        // Commit da transação
        await _unitOfWork.SaveChangesAsync();
    }
}
```

### Exemplo 2: Busca Avançada com Filtros Múltiplos

```csharp
public class ProductSearchModel : IQueryableCustom
{
    // Busca global em múltiplos campos
    [QueryOperator(WhereOperator.ContainsWithLikeForList, CaseSensitive = false)]
    public List<string>? GlobalTerms { get; set; }

    // Filtros de range de preço
    [QueryOperator(WhereOperator.GreaterThanOrEqualTo, FieldName = "Price")]
    public decimal? MinPrice { get; set; }

    [QueryOperator(WhereOperator.LessThanOrEqualTo, FieldName = "Price")]
    public decimal? MaxPrice { get; set; }

    // Filtros de data
    [QueryOperator(WhereOperator.GreaterThanOrEqualTo, FieldName = "DataCadastro")]
    public DateTimeOffset? CreatedAfter { get; set; }

    [QueryOperator(WhereOperator.LessThanOrEqualTo, FieldName = "DataCadastro")]
    public DateTimeOffset? CreatedBefore { get; set; }

    // Filtros booleanos
    [QueryOperator(WhereOperator.Equals)]
    public bool? InStock { get; set; }

    [QueryOperator(WhereOperator.Equals)]
    public bool? Featured { get; set; }

    // Filtros em relacionamentos
    [QueryOperator(WhereOperator.Equals, FieldName = "Category.Name")]
    public string? CategoryName { get; set; }

    [QueryOperator(WhereOperator.Contains, FieldName = "Supplier.CompanyName")]
    public string? SupplierName { get; set; }
}

public class AdvancedSearchService
{
    private readonly IRepository<Product> _repository;

    public AdvancedSearchService(IUnitOfWork unitOfWork)
    {
        _repository = unitOfWork.Repository<Product>();
    }

    public async Task<IPagedList<ProductDto>> SearchProductsAsync(
        ProductSearchModel filter,
        int pageNumber = 1,
        int pageSize = 20,
        string sortBy = "Name asc")
    {
        // A query é construída dinamicamente com base nos filtros preenchidos
        var result = await _repository.GetPagedListAsync(
            predicate: filter,
            selector: p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                Stock = p.Stock,
                CategoryName = p.Category.Name,
                SupplierName = p.Supplier.CompanyName,
                CreatedAt = p.DataCadastro,
                InStock = p.Stock > 0
            },
            orderBy: sortBy,
            include: query => query
                .Include(p => p.Category)
                .Include(p => p.Supplier),
            pageNumber: pageNumber,
            pageSize: pageSize
        );

        return result;
    }

    public async Task<List<ProductSummaryDto>> GetProductSummariesAsync(ProductSearchModel filter)
    {
        var query = _repository.GetAll(
            include: query => query
                .Include(p => p.Category)
                .Include(p => p.Supplier)
        );

        // Aplicar filtros dinâmicos
        query = query.Filter(filter);

        // Projeção para DTO
        var summaries = await query
            .Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                CategoryName = p.Category.Name
            })
            .ToListAsync();

        return summaries;
    }
}
```

### Exemplo 3: Relatórios e Agregações

```csharp
public class ReportService
{
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Product> _productRepository;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _orderRepository = unitOfWork.Repository<Order>();
        _productRepository = unitOfWork.Repository<Product>();
    }

    public async Task<SalesReportDto> GetSalesReportAsync(DateTimeOffset startDate, DateTimeOffset endDate)
    {
        // Query base com filtros
        var ordersQuery = _orderRepository.GetAll(
            predicate: o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.Status != OrderStatus.Cancelled,
            include: query => query.Include(o => o.Items)
        );

        var orders = await ordersQuery.ToListAsync();

        // Agregações
        var report = new SalesReportDto
        {
            StartDate = startDate,
            EndDate = endDate,
            TotalOrders = orders.Count,
            TotalRevenue = orders.Sum(o => o.Items.Sum(i => i.TotalPrice)),
            AverageOrderValue = orders.Any() ? orders.Average(o => o.Items.Sum(i => i.TotalPrice)) : 0,
            TotalItemsSold = orders.Sum(o => o.Items.Sum(i => i.Quantity))
        };

        // Top produtos
        var productSales = orders
            .SelectMany(o => o.Items)
            .GroupBy(i => i.ProductId)
            .Select(g => new ProductSalesDto
            {
                ProductId = g.Key,
                Quantity = g.Sum(i => i.Quantity),
                Revenue = g.Sum(i => i.TotalPrice)
            })
            .OrderByDescending(p => p.Revenue)
            .Take(10)
            .ToList();

        report.TopProducts = productSales;

        return report;
    }

    public async Task<List<CategorySalesDto>> GetCategorySalesAsync()
    {
        // Query complexa com agrupamento
        var categorySales = await _productRepository.GetAll(
            include: query => query
                .Include(p => p.Category)
                .Include(p => p.OrderItems)
        )
        .SelectMany(p => p.OrderItems, (product, orderItem) => new
        {
            CategoryName = product.Category.Name,
            Quantity = orderItem.Quantity,
            Revenue = orderItem.TotalPrice
        })
        .GroupBy(x => x.CategoryName)
        .Select(g => new CategorySalesDto
        {
            CategoryName = g.Key,
            TotalQuantity = g.Sum(x => x.Quantity),
            TotalRevenue = g.Sum(x => x.Revenue),
            AveragePrice = g.Average(x => x.Revenue / x.Quantity)
        })
        .OrderByDescending(c => c.TotalRevenue)
        .ToListAsync();

        return categorySales;
    }

    public async Task<InventoryReportDto> GetInventoryReportAsync()
    {
        var products = await _productRepository.GetAll().ToListAsync();

        var report = new InventoryReportDto
        {
            TotalProducts = products.Count,
            InStockProducts = products.Count(p => p.Stock > 0),
            OutOfStockProducts = products.Count(p => p.Stock == 0),
            LowStockProducts = products.Count(p => p.Stock > 0 && p.Stock < 10),
            TotalInventoryValue = products.Sum(p => p.Price * p.Stock)
        };

        // Produtos com baixo estoque
        report.LowStockItems = products
            .Where(p => p.Stock > 0 && p.Stock < 10)
            .Select(p => new LowStockItemDto
            {
                ProductId = p.Id,
                ProductName = p.Name,
                CurrentStock = p.Stock,
                RecommendedReorder = 50 - p.Stock
            })
            .ToList();

        return report;
    }
}
```

## API Reference

### IUnitOfWork

```csharp
public interface IUnitOfWork : IDisposable
{
    IRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
```

### IRepository&lt;T&gt;

#### Métodos Síncronos
- `void Add(T entity)` - Adiciona entidade
- `void AddRange(IEnumerable<T> entities)` - Adiciona múltiplas entidades
- `void Update(T entity)` - Atualiza entidade
- `void UpdateRange(IEnumerable<T> entities)` - Atualiza múltiplas entidades
- `void Remove(T entity)` - Remove entidade
- `void RemoveRange(IEnumerable<T> entities)` - Remove múltiplas entidades
- `void RemoveById(object id)` - Remove por ID

#### Métodos Assíncronos de Busca
- `Task<T?> FindAsync(object id)` - Busca por ID
- `Task<T?> GetFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)` - Primeira entidade que atende ao critério
- `Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)` - Verifica existência
- `Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)` - Conta registros

#### Queries
- `IQueryable<T> GetAll(Expression<Func<T, bool>>? predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include, bool disableTracking)` - Query base
- `Task<IPagedList<T>> GetPagedListAsync(IQueryableCustom predicate, string orderBy, int pageNumber, int pageSize)` - Paginação com filtros dinâmicos
- `Task<IPagedList<TResult>> GetPagedListAsync<TResult>(Expression<Func<T, TResult>> selector, ...)` - Paginação com projeção

### IPagedList&lt;T&gt;

```csharp
public interface IPagedList<T>
{
    int PageNumber { get; }
    int PageSize { get; }
    int TotalCount { get; }
    int TotalPages { get; }
    bool HasPreviousPage { get; }
    bool HasNextPage { get; }
    IList<T> Items { get; }
}
```

### Query Operators

```csharp
public enum WhereOperator
{
    Equals,
    NotEquals,
    GreaterThan,
    GreaterThanOrEqualTo,
    LessThan,
    LessThanOrEqualTo,
    Contains,
    StartsWith,
    EndsWith,
    ContainsWithLikeForList
}
```

### QueryOperatorAttribute

```csharp
[AttributeUsage(AttributeTargets.Property)]
public class QueryOperatorAttribute : Attribute
{
    public WhereOperator Operator { get; set; }
    public string? FieldName { get; set; }
    public bool CaseSensitive { get; set; } = true;
    public bool UseOr { get; set; } = false;
    public bool UseNot { get; set; } = false;
}
```

## Troubleshooting

### Problemas Comuns

#### 1. Filtros não funcionam

**Problema**: Filtros dinâmicos não aplicam os critérios

**Causa**: Modelo de filtro não implementa `IQueryableCustom`

**Solução**:
```csharp
// ✅ Correto
public class ProductFilter : IQueryableCustom
{
    [QueryOperator(WhereOperator.Contains)]
    public string? Name { get; set; }
}

// ❌ Incorreto
public class ProductFilter
{
    [QueryOperator(WhereOperator.Contains)]
    public string? Name { get; set; }
}
```

#### 2. Paginação retorna página vazia

**Problema**: `GetPagedListAsync` retorna `Items` vazio mas `TotalCount > 0`

**Causa**: `PageNumber` inválido ou maior que `TotalPages`

**Solução**:
```csharp
public async Task<IPagedList<Product>> GetProductsPagedAsync(int pageNumber, int pageSize)
{
    // Validar pageNumber
    if (pageNumber < 1)
    {
        pageNumber = 1;
    }

    var result = await _repository.GetPagedListAsync(
        predicate: null,
        orderBy: "Name asc",
        pageNumber: pageNumber,
        pageSize: pageSize
    );

    // Se página solicitada é maior que total de páginas, retornar última página
    if (pageNumber > result.TotalPages && result.TotalPages > 0)
    {
        return await GetProductsPagedAsync(result.TotalPages, pageSize);
    }

    return result;
}
```

#### 3. Includes não carregam relacionamentos

**Problema**: Propriedades navegacionais retornam `null`

**Causa**: Include não foi configurado ou tracking está desabilitado incorretamente

**Solução**:
```csharp
// ✅ Correto - Com Include
var order = await _repository.GetAll(
    predicate: o => o.Id == orderId,
    include: query => query
        .Include(o => o.Customer)
        .Include(o => o.Items)
    )
    .FirstOrDefaultAsync();

// ✅ Correto - ThenInclude para relacionamentos aninhados
var order = await _repository.GetAll(
    include: query => query
        .Include(o => o.Items)
            .ThenInclude(i => i.Product)
    )
    .FirstOrDefaultAsync();
```

#### 4. SaveChanges não persiste mudanças

**Problema**: Alterações não são salvas no banco

**Causa**: Esqueceu de chamar `await _unitOfWork.SaveChangesAsync()`

**Solução**:
```csharp
// ❌ Incorreto - Sem SaveChanges
public async Task UpdateProductAsync(string id, decimal newPrice)
{
    var product = await _repository.FindAsync(id);
    product.Price = newPrice;
    _repository.Update(product);
    // Mudanças não são persistidas!
}

// ✅ Correto - Com SaveChanges
public async Task UpdateProductAsync(string id, decimal newPrice)
{
    var product = await _repository.FindAsync(id);
    product.Price = newPrice;
    _repository.Update(product);
    await _unitOfWork.SaveChangesAsync(); // Persiste mudanças
}
```

#### 5. Performance ruim com grandes volumes

**Problema**: Queries lentas com muitos registros

**Causa**: Falta de projeção, includes desnecessários ou falta de índices

**Solução**:
```csharp
// ❌ Ruim - Traz entidade completa
var products = await _repository.GetAll().ToListAsync();

// ✅ Melhor - Usa projeção
var productDtos = await _repository.GetAll()
    .Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price
    })
    .ToListAsync();

// ✅ Melhor ainda - Usa paginação
var pagedResult = await _repository.GetPagedListAsync(
    selector: p => new ProductDto { /* ... */ },
    orderBy: "Name asc",
    pageNumber: 1,
    pageSize: 50
);
```

#### 6. ContainsWithLikeForList não funciona

**Problema**: Busca global não retorna resultados

**Causa**: Lista vazia ou null, ou campos não são string

**Solução**:
```csharp
public class GlobalSearchFilter : IQueryableCustom
{
    // ✅ Correto - Propriedade deve ser List<string>
    [QueryOperator(WhereOperator.ContainsWithLikeForList, CaseSensitive = false)]
    public List<string>? GlobalTerms { get; set; }
}

// Uso
public async Task<List<Product>> SearchAsync(string searchTerm)
{
    var filter = new GlobalSearchFilter
    {
        // Garantir que lista não é vazia
        GlobalTerms = !string.IsNullOrWhiteSpace(searchTerm)
            ? new List<string> { searchTerm }
            : null
    };

    return await _repository.GetAll()
        .Filter(filter)
        .ToListAsync();
}
```

### Logs e Debugging

Para debug de queries EF Core:

```csharp
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}

// Logs mostrarão SQL gerado:
// Executing DbCommand [CommandType='Text', CommandTimeout='30']
// SELECT [p].[Id], [p].[Name], [p].[Price]
// FROM [Products] AS [p]
// WHERE [p].[Price] >= @__MinPrice_0
```

## Changelog

Ver arquivo [CHANGELOG.md](CHANGELOG.md) para histórico detalhado de alterações.

---

## 📞 Suporte

Para dúvidas, issues ou contribuições:
- 🐛 **Issues**: [GitHub Issues](https://github.com/nuuvify/CommonPack/issues)
- 📧 **Email**: [suporte@zocate.li](mailto:suporte@zocate.li)
- 📖 **Documentação**: [Wiki do Projeto](https://github.com/nuuvify/CommonPack/wiki)
- 📂 **Exemplos**: Ver pasta [Examples](./Examples/) com casos de uso detalhados

---
**Nuuvify CommonPack** - Construindo soluções robustas para .NET 🚀
