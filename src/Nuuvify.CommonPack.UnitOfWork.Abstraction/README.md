# Nuuvify.CommonPack.UnitOfWork

[![.NET Standard 2.1](https://img.shields.io/badge/.NET%20Standard-2.1-blue.svg)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-green.svg)](https://docs.microsoft.com/en-us/ef/core/)
[![Dynamic Queries](https://img.shields.io/badge/Dynamic-Queries-orange.svg)]()
[![NuGet Version](https://img.shields.io/nuget/v/Nuuvify.CommonPack.UnitOfWork.svg)](https://www.nuget.org/packages/Nuuvify.CommonPack.UnitOfWork/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Uma biblioteca .NET poderosa e flexível para implementação do padrão **Unit of Work** com **consultas dinâmicas**, **filtros avançados**, **paginação** e **ordenação**. Projetada especificamente para Entity Framework Core e aplicações empresariais.

## 🚀 Características Principais

- **Unit of Work Pattern**: Implementação completa do padrão Unit of Work
- **Consultas Dinâmicas**: Sistema avançado de filtros com Expression Trees
- **13 Operadores de Filtro**: From `Equals` to `ContainsWithLikeForList` (OR-based search)
- **Paginação Inteligente**: Paginação otimizada com metadados completos
- **Ordenação Flexível**: Múltiplos critérios de ordenação
- **Thread-Safe**: Totalmente thread-safe para aplicações concorrentes
- **Type-Safe**: Validação em tempo de compilação com enums tipados
- **Performance Otimizada**: Queries eficientes com projection e defer loading
- **Nullable Support**: Suporte completo a Nullable Reference Types
- **Expression Validation**: Validação robusta de expressões e parâmetros

## 🆕 O Que Há de Novo na v3.2.0 (2025-10-28)

### ✅ Correções Críticas

Esta versão inclui **3 correções críticas** no operador `ContainsWithLikeForList`:

1. **🐛 Bug Expression.Constant(false)** - CORRIGIDO
   - **Problema**: Listas vazias geravam `WHERE 0 = 1` (SQL inválido)
   - **Solução**: Retornar `null` permite ignorar filtros vazios corretamente

2. **🐛 Bug Null Expression** - CORRIGIDO
   - **Problema**: Expressões nulas causavam crashes em `Expression.And/Or`
   - **Solução**: Adicionado `if (actualExpression == null) continue;`

3. **🐛 Bug UnaryExpression Wrapping** - CORRIGIDO
   - **Problema**: `FilterBy` encapsulado em `UnaryExpression` impedia acesso à lista
   - **Solução**: Unwrap automático do `UnaryExpression` para extrair `ConstantExpression`
   - **SQL Gerado**:
     ```sql
     -- Antes (bug): SELECT * FROM Products (sem WHERE)
     -- Depois (✅): SELECT * FROM Products WHERE Name LIKE '%iPhone%' OR Name LIKE '%Samsung%'
     ```

### 🎨 Melhorias de Código

- **Partial Classes**: `FiltersExtensions` dividido em arquivos separados
  - `FiltersExtensions.cs`: API pública com documentação completa
  - `FiltersExtensions.Private.cs`: Implementação privada

- **Documentação XML**: Exemplos práticos em todos os métodos públicos
- **Code Cleanup**: Comentários inline substituídos por documentação XML
- **100% Tested**: Todos os 12 testes passando com cobertura completa

> 📖 **Veja o [CHANGELOG.md](./CHANGELOG.md) para detalhes técnicos completos**

## 📦 Instalação

```bash
# Pacote principal com implementações
dotnet add package Nuuvify.CommonPack.UnitOfWork

# Pacote com abstrações (interfaces)
dotnet add package Nuuvify.CommonPack.UnitOfWork.Abstraction
```

## ⚙️ Configuração Rápida

### 1. Configuração no Program.cs (.NET 8)

```csharp
using Nuuvify.CommonPack.UnitOfWork.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Registrar Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Registrar Unit of Work
builder.Services.AddUnitOfWork<AppDbContext>();

var app = builder.Build();
```

### 2. Configuração com Options Pattern

```csharp
builder.Services.Configure<UnitOfWorkOptions>(options =>
{
    options.DefaultPageSize = 20;
    options.MaxPageSize = 100;
    options.EnableQuerySplitting = true;
    options.EnableChangeTracking = false; // Para queries de leitura
});

builder.Services.AddUnitOfWork<AppDbContext>();
```

## 🎯 Casos de Uso Completos

### Modelo de Dados de Exemplo

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastUpdate { get; set; }
    public List<string> Tags { get; set; } = new();
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public enum OrderStatus { Pending, Confirmed, Shipped, Delivered, Cancelled }
```

### Modelo de Filtro Completo - Demonstrando Todos os Operadores

```csharp
public class ProductSearchModel : IQueryableCustom
{
    // ===== OPERADORES DE IGUALDADE =====

    /// <summary>
    /// Filtro por ID exato - Operador: Equals
    /// Exemplo: WHERE Id = @value
    /// </summary>
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Id))]
    public int? ProductId { get; set; }

    /// <summary>
    /// Filtro por categoria exata - Operador: Equals (case-insensitive)
    /// Exemplo: WHERE UPPER(Category) = UPPER(@value)
    /// </summary>
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Category), CaseSensitive = false)]
    public string? CategoryExact { get; set; }

    /// <summary>
    /// Exclusão por categoria - Operador: NotEquals
    /// Exemplo: WHERE Category <> @value
    /// </summary>
    [QueryOperator(Operator = WhereOperator.NotEquals, HasName = nameof(Product.Category))]
    public string? ExcludeCategory { get; set; }

    // ===== OPERADORES DE COMPARAÇÃO NUMÉRICA =====

    /// <summary>
    /// Preço mínimo - Operador: GreaterThanOrEqualTo
    /// Exemplo: WHERE Price >= @value
    /// </summary>
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualTo, HasName = nameof(Product.Price))]
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Preço máximo - Operador: LessThanOrEqualTo
    /// Exemplo: WHERE Price <= @value
    /// </summary>
    [QueryOperator(Operator = WhereOperator.LessThanOrEqualTo, HasName = nameof(Product.Price))]
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Produtos com preço maior que - Operador: GreaterThan
    /// Exemplo: WHERE Price > @value
    /// </summary>
    [QueryOperator(Operator = WhereOperator.GreaterThan, HasName = nameof(Product.Price))]
    public decimal? PriceGreaterThan { get; set; }

    /// <summary>
    /// Produtos com preço menor que - Operador: LessThan
    /// Exemplo: WHERE Price < @value
    /// </summary>
    [QueryOperator(Operator = WhereOperator.LessThan, HasName = nameof(Product.Price))]
    public decimal? PriceLessThan { get; set; }

    // ===== OPERADORES DE COMPARAÇÃO COM NULLABLE =====

    /// <summary>
    /// Estoque mínimo (com suporte nullable) - Operador: GreaterThanOrEqualWhenNullable
    /// Exemplo: WHERE Stock >= @value (com conversão automática de nullable)
    /// </summary>
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualWhenNullable, HasName = nameof(Product.Stock))]
    public int? MinStock { get; set; }

    /// <summary>
    /// Estoque máximo (com suporte nullable) - Operador: LessThanOrEqualWhenNullable
    /// Exemplo: WHERE Stock <= @value (com conversão automática de nullable)
    /// </summary>
    [QueryOperator(Operator = WhereOperator.LessThanOrEqualWhenNullable, HasName = nameof(Product.Stock))]
    public int? MaxStock { get; set; }

    /// <summary>
    /// Data de atualização (com suporte nullable) - Operador: EqualsWhenNullable
    /// Exemplo: WHERE LastUpdate = @value (com conversão automática de nullable)
    /// </summary>
    [QueryOperator(Operator = WhereOperator.EqualsWhenNullable, HasName = nameof(Product.LastUpdate))]
    public DateTime? LastUpdateDate { get; set; }

    // ===== OPERADORES DE TEXTO =====

    /// <summary>
    /// Busca por nome (contém) - Operador: Contains
    /// Exemplo: WHERE Name.Contains(@value)
    /// </summary>
    [QueryOperator(Operator = WhereOperator.Contains, HasName = nameof(Product.Name), CaseSensitive = false)]
    public string? NameSearch { get; set; }

    /// <summary>
    /// Nome que inicia com - Operador: StartsWith
    /// Exemplo: WHERE Name.StartsWith(@value)
    /// </summary>
    [QueryOperator(Operator = WhereOperator.StartsWith, HasName = nameof(Product.Name), CaseSensitive = false)]
    public string? NameStartsWith { get; set; }

    /// <summary>
    /// Busca em múltiplos campos - Operador: ContainsWithLikeForList (NOVO!)
    /// Exemplo: WHERE (Name.Contains(@value1) OR Name.Contains(@value2) OR ...)
    ///
    /// ✨ Este é o operador mais poderoso - permite busca OR em listas
    /// Use para implementar busca global, tags, categorias múltiplas, etc.
    /// </summary>
    [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList, HasName = nameof(Product.Name))]
    public List<string>? GlobalSearch { get; set; }

    // ===== OPERADORES LÓGICOS E DE COMBINAÇÃO =====

    /// <summary>
    /// Filtro com OR - demonstra uso de UseOr = true
    /// Exemplo: WHERE (previous_conditions) OR (Category = @value)
    /// </summary>
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Category), UseOr = true)]
    public string? AlternativeCategory { get; set; }

    /// <summary>
    /// Filtro com NOT - demonstra uso de UseNot = true
    /// Exemplo: WHERE NOT (IsActive = @value)
    /// </summary>
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.IsActive), UseNot = true)]
    public bool? ExcludeActive { get; set; }

    // ===== PAGINAÇÃO E ORDENAÇÃO =====

    /// <summary>
    /// Página atual (1-based)
    /// </summary>
    [Key]
    public int PageIndex { get; set; } = 1;

    /// <summary>
    /// Tamanho da página
    /// </summary>
    [Key]
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Ordenação: "A-Name, D-Price, A-CreatedAt"
    /// Formato: "D-Campo" (Descendente) ou "A-Campo" (Ascendente)
    /// Suporta múltiplos campos separados por vírgula
    /// </summary>
    public string Sort { get; set; } = string.Empty;
}
```

### Modelo de Filtro para Pedidos - Casos Avançados

```csharp
public class OrderSearchModel : IQueryableCustom
{
    // ===== FILTROS DE DATA COM RANGES =====

    /// <summary>
    /// Data inicial do pedido
    /// </summary>
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualTo, HasName = nameof(Order.OrderDate))]
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// Data final do pedido
    /// </summary>
    [QueryOperator(Operator = WhereOperator.LessThanOrEqualTo, HasName = nameof(Order.OrderDate))]
    public DateTime? EndDate { get; set; }

    // ===== FILTROS DE TEXTO AVANÇADOS =====

    /// <summary>
    /// Busca por múltiplos nomes de cliente (OR)
    /// Exemplo: WHERE CustomerName.Contains("João") OR CustomerName.Contains("Maria")
    /// </summary>
    [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList, HasName = nameof(Order.CustomerName))]
    public List<string>? CustomerNames { get; set; }

    /// <summary>
    /// Email do cliente que inicia com
    /// </summary>
    [QueryOperator(Operator = WhereOperator.StartsWith, HasName = nameof(Order.CustomerEmail), CaseSensitive = false)]
    public string? EmailDomain { get; set; }

    // ===== FILTROS DE VALOR =====

    /// <summary>
    /// Valor mínimo do pedido
    /// </summary>
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualTo, HasName = nameof(Order.TotalAmount))]
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Valor máximo do pedido
    /// </summary>
    [QueryOperator(Operator = WhereOperator.LessThanOrEqualTo, HasName = nameof(Order.TotalAmount))]
    public decimal? MaxAmount { get; set; }

    // ===== FILTROS DE ENUM E STATUS =====

    /// <summary>
    /// Status específico do pedido
    /// </summary>
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Order.Status))]
    public OrderStatus? Status { get; set; }

    /// <summary>
    /// Excluir pedidos cancelados
    /// </summary>
    [QueryOperator(Operator = WhereOperator.NotEquals, HasName = nameof(Order.Status))]
    public OrderStatus? ExcludeStatus { get; set; } = OrderStatus.Cancelled;

    // ===== FILTROS DE DATA NULLABLE =====

    /// <summary>
    /// Pedidos que ainda não foram enviados (ShippedDate = null)
    /// Para filtrar NULL, deixe ShippedDateExists = false e não preencha ShippedDate
    /// </summary>
    [QueryOperator(Operator = WhereOperator.EqualsWhenNullable, HasName = nameof(Order.ShippedDate))]
    public DateTime? ShippedDate { get; set; }

    // ===== COMBINAÇÕES LÓGICAS AVANÇADAS =====

    /// <summary>
    /// Busca alternativa com OR - pedidos urgentes OU de valor alto
    /// </summary>
    [QueryOperator(Operator = WhereOperator.GreaterThan, HasName = nameof(Order.TotalAmount), UseOr = true)]
    public decimal? HighValueAlternative { get; set; }

    /// <summary>
    /// Exclusão com NOT - todos exceto os pendentes
    /// </summary>
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Order.Status), UseNot = true)]
    public OrderStatus? NotStatus { get; set; }

    // ===== PAGINAÇÃO E ORDENAÇÃO =====
    [Key]
    public int PageIndex { get; set; } = 1;

    [Key]
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// Exemplos de ordenação:
    /// - "D-OrderDate" - Mais recentes primeiro (Descendente)
    /// - "D-TotalAmount, D-OrderDate" - Por valor e data (ambos descendentes)
    /// - "A-CustomerName, D-OrderDate" - Por cliente (ascendente) e data (descendente)
    /// </summary>
    public string Sort { get; set; } = "D-OrderDate";
}
```

## 🏗️ Implementação em Controllers/Services

### Controller Básico com Todos os Recursos

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IUnitOfWork<AppDbContext> unitOfWork, ILogger<ProductsController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Busca produtos com filtros dinâmicos e paginação
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<IPagedList<ProductDto>>> SearchProducts([FromQuery] ProductSearchModel filter)
    {
        try
        {
            // ✅ Query com filtros dinâmicos, paginação e ordenação
            var query = _unitOfWork.Repository<Product>()
                .GetAll()
                .Where(p => p.IsActive) // Filtro fixo
                .Filter(filter) // Filtros dinâmicos
                .Sort(filter.Sort) // Ordenação
                .Select(p => new ProductDto // Projeção para DTO
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                });

            var result = await query.ToPagedListAsync(filter.PageIndex, filter.PageSize);

            _logger.LogInformation("Produtos encontrados: {Count}/{Total}. Página: {Page}/{TotalPages}",
                result.Items.Count, result.TotalCount, result.PageIndex, result.TotalPages);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos com filtros: {@Filters}", filter);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Busca produtos sem paginação (para dropdown, autocomplete, etc.)
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<List<ProductDto>>> GetProductsList([FromQuery] ProductSearchModel filter)
    {
        try
        {
            // ✅ Query sem paginação - apenas filtros dinâmicos
            var products = await _unitOfWork.Repository<Product>()
                .GetAll()
                .Where(p => p.IsActive)
                .Filter(filter) // Apenas filtros, sem paginação
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price
                })
                .Take(100) // Limite para segurança
                .ToListAsync();

            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar produtos");
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Demonstra o novo operador ContainsWithLikeForList
    /// </summary>
    [HttpGet("global-search")]
    public async Task<ActionResult<IPagedList<ProductDto>>> GlobalSearch([FromQuery] string[] terms)
    {
        var filter = new ProductSearchModel
        {
            GlobalSearch = terms?.ToList(), // Busca OR em múltiplos termos
            Sort = "A-Name",
            PageIndex = 1,
            PageSize = 10
        };

        var query = _unitOfWork.Repository<Product>()
            .GetAll()
            .Filter(filter)
            .Sort(filter.Sort)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price
            });

        var result = await query.ToPagedListAsync(filter.PageIndex, filter.PageSize);

        return Ok(result);
    }

    /// <summary>
    /// Criar produto com Unit of Work pattern
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductRequest request)
    {
        try
        {
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                Price = request.Price,
                Stock = request.Stock,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // ✅ Unit of Work - adiciona e salva em uma transação
            await _unitOfWork.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var dto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price
            };

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto: {@Request}", request);
            return StatusCode(500, "Erro interno do servidor");
        }
    }

    /// <summary>
    /// Buscar produto por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id)
    {
        var product = await _unitOfWork.FindByIdAsync<Product>(id);

        if (product == null)
            return NotFound();

        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price,
            Stock = product.Stock
        };

        return Ok(dto);
    }
}
```

### Service Avançado - Demonstrando Casos Complexos

```csharp
public class OrderService
{
    private readonly IUnitOfWork<AppDbContext> _unitOfWork;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork<AppDbContext> unitOfWork, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Busca avançada de pedidos - demonstra filtros complexos
    /// </summary>
    public async Task<IPagedList<OrderDto>> SearchOrdersAsync(OrderSearchModel filter)
    {
        try
        {
            // ✅ Demonstra query complexa com joins e filtros dinâmicos
            var query = _unitOfWork.Repository<Order>()
                .GetAll(
                    predicate: o => !o.Status.Equals(OrderStatus.Cancelled) || filter.ExcludeStatus != OrderStatus.Cancelled,
                    include: source => source.Include(o => o.Items))
                .Filter(filter)
                .Sort(filter.Sort)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    CustomerName = o.CustomerName,
                    CustomerEmail = o.CustomerEmail,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status.ToString(),
                    OrderDate = o.OrderDate,
                    ItemCount = o.Items.Count
                });

            var result = await query.ToPagedListAsync(filter.PageIndex, filter.PageSize);

            _logger.LogInformation("Pedidos encontrados: {Count}/{Total}", result.Items.Count, result.TotalCount);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedidos: {@Filter}", filter);
            throw;
        }
    }

    /// <summary>
    /// Demonstra agregações e relatórios
    /// </summary>
    public async Task<OrderReportDto> GetOrderReportAsync(OrderSearchModel filter)
    {
        // ✅ Consulta sem paginação para relatórios
        var orders = await _unitOfWork.Repository<Order>()
            .GetAll()
            .Filter(filter) // Aplica apenas filtros, sem paginação
            .ToListAsync();

        var report = new OrderReportDto
        {
            TotalOrders = orders.Count,
            TotalAmount = orders.Sum(o => o.TotalAmount),
            AverageAmount = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,
            OrdersByStatus = orders
                .GroupBy(o => o.Status)
                .ToDictionary(g => g.Key.ToString(), g => g.Count())
        };

        return report;
    }

    /// <summary>
    /// Demonstra transações complexas com Unit of Work
    /// </summary>
    public async Task<OrderDto> CreateOrderAsync(CreateOrderRequest request)
    {
        // ✅ Unit of Work gerencia a transação automaticamente
        try
        {
            var order = new Order
            {
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            };

            // Adiciona o pedido
            await _unitOfWork.AddAsync(order);

            // Adiciona os itens (múltiplas operações na mesma transação)
            foreach (var itemRequest in request.Items)
            {
                var product = await _unitOfWork.FindByIdAsync<Product>(itemRequest.ProductId);
                if (product == null)
                    throw new InvalidOperationException($"Produto {itemRequest.ProductId} não encontrado");

                var item = new OrderItem
                {
                    Order = order,
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.Price
                };

                await _unitOfWork.AddAsync(item);
                order.TotalAmount += item.Quantity * item.UnitPrice;

                // Atualiza estoque
                product.Stock -= itemRequest.Quantity;
                _unitOfWork.Update(product);
            }

            // ✅ Salva tudo em uma única transação
            await _unitOfWork.SaveChangesAsync();

            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                OrderDate = order.OrderDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido: {@Request}", request);
            throw;
        }
    }
}
```

## 📋 Referência Completa dos Operadores

| Operador                           | Descrição              | Exemplo SQL                                          | Uso Comum                    |
| ---------------------------------- | ---------------------- | ---------------------------------------------------- | ---------------------------- |
| **Equals**                         | Igualdade exata        | `WHERE Field = @value`                               | IDs, status, categorias      |
| **NotEquals**                      | Diferente de           | `WHERE Field <> @value`                              | Exclusões, filtros negativos |
| **GreaterThan**                    | Maior que              | `WHERE Field > @value`                               | Idades, valores mínimos      |
| **LessThan**                       | Menor que              | `WHERE Field < @value`                               | Limites máximos              |
| **GreaterThanOrEqualTo**           | Maior ou igual         | `WHERE Field >= @value`                              | Datas início, preços mín     |
| **LessThanOrEqualTo**              | Menor ou igual         | `WHERE Field <= @value`                              | Datas fim, preços máx        |
| **Contains**                       | Contém texto           | `WHERE Field.Contains(@value)`                       | Buscas de texto              |
| **StartsWith**                     | Inicia com             | `WHERE Field.StartsWith(@value)`                     | Prefixos, códigos            |
| **GreaterThanOrEqualWhenNullable** | Maior/igual (nullable) | `WHERE Field >= @value`                              | Campos nullable              |
| **LessThanOrEqualWhenNullable**    | Menor/igual (nullable) | `WHERE Field <= @value`                              | Campos nullable              |
| **EqualsWhenNullable**             | Igualdade (nullable)   | `WHERE Field = @value`                               | Campos nullable              |
| **ContainsWithLikeForList**        | OR múltiplo            | `WHERE (Field.Contains(@v1) OR Field.Contains(@v2))` | **🆕 Busca global, tags**     |

### Modificadores de Comportamento

| Propriedade       | Descrição                           | Exemplo                 |
| ----------------- | ----------------------------------- | ----------------------- |
| **CaseSensitive** | Controla sensibilidade a maiúsculas | `CaseSensitive = false` |
| **UseOr**         | Usa OR ao invés de AND              | `UseOr = true`          |
| **UseNot**        | Nega a condição                     | `UseNot = true`         |

## 🎨 Exemplos de Uso dos Operadores

### 1. Operador ContainsWithLikeForList - Busca Global

```csharp
// ✨ NOVO OPERADOR - Mais poderoso para buscas múltiplas
public class ProductGlobalSearchModel : IQueryableCustom
{
    /// <summary>
    /// Busca por múltiplos termos com OR
    /// Exemplo: ["iPhone", "Samsung"] resulta em:
    /// WHERE (Name.Contains("iPhone") OR Name.Contains("Samsung"))
    /// </summary>
    [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList, HasName = nameof(Product.Name))]
    public List<string>? SearchTerms { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = string.Empty;
}

// Uso em controller
[HttpGet("global-search")]
public async Task<ActionResult> GlobalSearch([FromQuery] string[] terms)
{
    var filter = new ProductGlobalSearchModel
    {
        SearchTerms = terms.ToList(), // ["Apple", "Samsung", "Xiaomi"]
        PageIndex = 1,
        PageSize = 10
    };

    // Resulta em: WHERE (Name.Contains("Apple") OR Name.Contains("Samsung") OR Name.Contains("Xiaomi"))
    var query = _unitOfWork.Repository<Product>()
        .GetAll()
        .Filter(filter)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name
        });

    var result = await query.ToPagedListAsync(filter.PageIndex, filter.PageSize);

    return Ok(result);
}
```

### 2. Filtros de Range (Datas e Valores)

```csharp
public class ProductPriceRangeModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualTo, HasName = nameof(Product.Price))]
    public decimal? MinPrice { get; set; }

    [QueryOperator(Operator = WhereOperator.LessThanOrEqualTo, HasName = nameof(Product.Price))]
    public decimal? MaxPrice { get; set; }

    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualTo, HasName = nameof(Product.CreatedAt))]
    public DateTime? StartDate { get; set; }

    [QueryOperator(Operator = WhereOperator.LessThanOrEqualTo, HasName = nameof(Product.CreatedAt))]
    public DateTime? EndDate { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = string.Empty;
}

// Uso: ?MinPrice=100&MaxPrice=500&StartDate=2024-01-01&EndDate=2024-12-31
// Resulta em: WHERE Price >= 100 AND Price <= 500 AND CreatedAt >= '2024-01-01' AND CreatedAt <= '2024-12-31'
```

### 3. Filtros com Lógica Complexa (OR e NOT)

```csharp
public class ProductComplexFilterModel : IQueryableCustom
{
    // Categoria principal
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Category))]
    public string? PrimaryCategory { get; set; }

    // Categoria alternativa (OR)
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Category), UseOr = true)]
    public string? AlternativeCategory { get; set; }

    // Excluir produtos inativos (NOT)
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.IsActive), UseNot = true)]
    public bool? ExcludeInactive { get; set; } = false; // NOT (IsActive = false) => produtos ativos

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = string.Empty;
}

// Uso: ?PrimaryCategory=Electronics&AlternativeCategory=Gadgets&ExcludeInactive=false
// Resulta em: WHERE (Category = 'Electronics' OR Category = 'Gadgets') AND NOT (IsActive = false)
```

### 4. Filtros Case-Insensitive

```csharp
public class ProductTextSearchModel : IQueryableCustom
{
    // Busca case-sensitive (padrão)
    [QueryOperator(Operator = WhereOperator.Contains, HasName = nameof(Product.Name))]
    public string? NameExact { get; set; }

    // Busca case-insensitive
    [QueryOperator(Operator = WhereOperator.Contains, HasName = nameof(Product.Name), CaseSensitive = false)]
    public string? NameIgnoreCase { get; set; }

    // Categoria case-insensitive
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Category), CaseSensitive = false)]
    public string? Category { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = string.Empty;
}

// NameIgnoreCase=IPHONE resulta em: WHERE UPPER(Name).Contains(UPPER('IPHONE'))
// Category=electronics resulta em: WHERE UPPER(Category) = UPPER('electronics')
```

### 5. Filtros com Campos Nullable

```csharp
public class ProductNullableFilterModel : IQueryableCustom
{
    // Data de última atualização (pode ser null)
    [QueryOperator(Operator = WhereOperator.EqualsWhenNullable, HasName = nameof(Product.LastUpdate))]
    public DateTime? LastUpdate { get; set; }

    // Estoque mínimo (nullable-safe)
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualWhenNullable, HasName = nameof(Product.Stock))]
    public int? MinStock { get; set; }

    // Preço máximo (nullable-safe)
    [QueryOperator(Operator = WhereOperator.LessThanOrEqualWhenNullable, HasName = nameof(Product.Price))]
    public decimal? MaxPrice { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string Sort { get; set; } = string.Empty;
}
```

## 🔧 Ordenação Avançada

### Formato do Sort

A ordenação utiliza o formato de **prefixo** para indicar a direção:

- **D-** = Descendente (Decrescente) - Ex: `D-Price` resulta em `ORDER BY Price DESC`
- **A-** = Ascendente (Crescente) - Ex: `A-Name` resulta em `ORDER BY Name ASC`

**Sintaxe**: `"[D|A]-NomePropriedade"`

**Múltiplos campos**: Separe por vírgula - Ex: `"A-Category, D-Price, A-Name"`

### Exemplos de Ordenação Múltipla

```csharp
// Ordenação simples - Descendente (D-)
filter.Sort = "D-Name";                      // ORDER BY Name DESC
filter.Sort = "D-Price";                     // ORDER BY Price DESC

// Ordenação simples - Ascendente (A-)
filter.Sort = "A-Name";                      // ORDER BY Name ASC
filter.Sort = "A-Price";                     // ORDER BY Price ASC

// Ordenação múltipla
filter.Sort = "A-Category, D-Price";         // ORDER BY Category ASC, Price DESC
filter.Sort = "D-IsActive, A-Name, D-CreatedAt"; // Múltiplos campos

// Casos especiais
filter.Sort = "Price";                       // ⚠️ INVÁLIDO - deve especificar D- ou A-
filter.Sort = "";                            // Sem ordenação (ordem do banco)
```

### Service com Ordenação Dinâmica

```csharp
public async Task<IPagedList<ProductDto>> GetProductsWithSortingAsync(
    string category,
    string sortBy = "name",
    string direction = "A", // "A" para Ascendente ou "D" para Descendente
    int page = 1,
    int pageSize = 20)
{
    var filter = new ProductSearchModel
    {
        CategoryExact = category,
        Sort = $"{direction}-{sortBy}", // Ex: "A-Name" ou "D-Price"
        PageIndex = page,
        PageSize = pageSize
    };

    var query = _unitOfWork.Repository<Product>()
        .GetAll()
        .Where(p => p.IsActive)
        .Filter(filter)
        .Sort(filter.Sort)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Category = p.Category,
            Price = p.Price,
            CreatedAt = p.CreatedAt
        });

    return await query.ToPagedListAsync(filter.PageIndex, filter.PageSize);
}
```

## 📄 DTOs e Modelos de Resposta

```csharp
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
}

// IPagedList<T> é retornado por ToPagedListAsync()
// Contém propriedades úteis para paginação:
// - Items: Lista de itens da página atual
// - PageIndex: Página atual (1-based)
// - PageSize: Tamanho da página
// - TotalCount: Total de registros
// - TotalPages: Total de páginas
// - HasPreviousPage: Indica se há página anterior
// - HasNextPage: Indica se há próxima página
// - IndexFrom: Índice inicial (default 0)

public class OrderReportDto
{
    public int TotalOrders { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AverageAmount { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
}
```

## 🚀 Performance e Otimizações

### 1. Queries Eficientes

```csharp
// ✅ BOM: Projeção para DTO
var products = await _unitOfWork.Repository<Product>()
    .GetAll()
    .Filter(filter)
    .Select(p => new ProductDto { Id = p.Id, Name = p.Name }) // Apenas campos necessários
    .ToListAsync();

// ❌ EVITAR: Carregar entidade completa desnecessariamente
var products = await _unitOfWork.Repository<Product>()
    .GetAll()
    .Filter(filter)
    .ToListAsync(); // Carrega todos os campos
```

### 2. Paginação Otimizada

```csharp
// ✅ BOM: Paginação direta no banco
var query = _unitOfWork.Repository<Product>()
    .GetAll()
    .Filter(filter)
    .Select(p => new ProductDto { Id = p.Id, Name = p.Name });

var result = await query.ToPagedListAsync(filter.PageIndex, filter.PageSize); // Skip/Take no SQL

// ❌ EVITAR: Paginação em memória
var allProducts = await _unitOfWork.Repository<Product>()
    .GetAll()
    .Filter(filter)
    .ToListAsync();
var pagedProducts = allProducts.Skip(skip).Take(take); // Carrega tudo na memória
```

### 3. Includes Inteligentes

```csharp
// ✅ BOM: Include apenas quando necessário
var orders = await _unitOfWork.Repository<Order>()
    .GetAll(include: source => source
        .Include(o => o.Items.Take(5))) // Limit related data
    .Filter(filter)
    .ToListAsync();

// ❌ EVITAR: Include desnecessário
var orders = await _unitOfWork.Repository<Order>()
    .GetAll(include: source => source
        .Include(o => o.Items)
        .Include(o => o.Customer)
        .Include(o => o.ShippingAddress)) // Dados não utilizados
    .Filter(filter)
    .ToListAsync();
```
    .ToListAsync();
```

## 🧪 Testes

### Exemplo de Teste Unitário

```csharp
[Test]
public async Task Filter_WithContainsWithLikeForList_ShouldReturnCorrectResults()
{
    // Arrange
    var products = new List<Product>
    {
        new() { Id = 1, Name = "iPhone 15 Pro", Category = "Electronics" },
        new() { Id = 2, Name = "Samsung Galaxy S24", Category = "Electronics" },
        new() { Id = 3, Name = "iPad Air", Category = "Tablets" },
        new() { Id = 4, Name = "MacBook Pro", Category = "Laptops" }
    };

    var filter = new ProductSearchModel
    {
        GlobalSearch = new List<string> { "iPhone", "Samsung" },
        PageIndex = 1,
        PageSize = 10
    };

    // Act
    var result = products.AsQueryable()
        .Filter(filter)
        .ToList();

    // Assert
    Assert.That(result.Count, Is.EqualTo(2));
    Assert.That(result.Any(p => p.Name.Contains("iPhone")), Is.True);
    Assert.That(result.Any(p => p.Name.Contains("Samsung")), Is.True);
}
```

### Teste de Integração

```csharp
[Test]
public async Task SearchProducts_WithComplexFilters_ShouldReturnPagedResults()
{
    // Arrange
    using var context = new TestDbContext();
    var unitOfWork = new UnitOfWork<TestDbContext>(context);

    await SeedTestData(context);

    var filter = new ProductSearchModel
    {
        MinPrice = 100,
        MaxPrice = 1000,
        NameSearch = "Pro",
        Sort = "D-Price",
        PageIndex = 1,
        PageSize = 5
    };

    // Act
    var query = unitOfWork.Repository<Product>()
        .GetAll()
        .Filter(filter)
        .Sort(filter.Sort)
        .Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price
        });

    var result = await query.ToPagedListAsync(filter.PageIndex, filter.PageSize);

    // Assert
    Assert.That(result.Items.Count, Is.LessThanOrEqualTo(5));
    Assert.That(result.TotalCount, Is.GreaterThan(0));
    Assert.That(result.Items.All(p => p.Price >= 100 && p.Price <= 1000), Is.True);
    Assert.That(result.Items.All(p => p.Name.Contains("Pro")), Is.True);
}
```

## 📊 Dependências

- **.NET Standard 2.1** - Framework base
- **Entity Framework Core** - ORM principal
- **Microsoft.Extensions.DependencyInjection** - Container de DI
- **System.Linq.Expressions** - Expression Trees
- **Nuuvify.CommonPack.Extensions** - Extensões úteis

## 🔒 Thread Safety

A biblioteca é **completamente thread-safe**:

```csharp
// ✅ Seguro usar como Singleton
services.AddSingleton<IUnitOfWork<AppDbContext>>();

// ✅ Seguro usar como Scoped (recomendado para web)
services.AddScoped<IUnitOfWork<AppDbContext>>();

// ✅ Múltiplas threads podem usar simultaneamente
var tasks = Enumerable.Range(1, 10).Select(async i =>
{
    var filter = new ProductSearchModel { NameSearch = $"Product {i}" };
    return await _unitOfWork.Repository<Product>()
        .GetAll()
        .Filter(filter)
        .ToListAsync();
});

var results = await Task.WhenAll(tasks);
```

## 🔧 Configurações Avançadas

### Options Pattern

```csharp
public class UnitOfWorkOptions
{
    public int DefaultPageSize { get; set; } = 20;
    public int MaxPageSize { get; set; } = 100;
    public bool EnableQuerySplitting { get; set; } = true;
    public bool EnableChangeTracking { get; set; } = true;
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
}

// Configuração
builder.Services.Configure<UnitOfWorkOptions>(options =>
{
    options.DefaultPageSize = 25;
    options.MaxPageSize = 200;
});
```

### Interceptors Personalizados

```csharp
public class QueryInterceptor : IQueryInterceptor
{
    public IQueryable<T> BeforeQuery<T>(IQueryable<T> query) where T : class
    {
        // Aplicar filtros globais, auditoria, etc.
        if (typeof(T).GetInterface(nameof(ISoftDelete)) != null)
        {
            query = query.Where(e => !((ISoftDelete)e).IsDeleted);
        }

        return query;
    }
}

// Registro
builder.Services.AddScoped<IQueryInterceptor, QueryInterceptor>();
```

## 🔍 Troubleshooting

### Problema: Filtro ContainsWithLikeForList não gera WHERE clause

**Sintoma**: Query retorna todos os registros ignorando o filtro de lista.

**SQL Gerado**:
```sql
SELECT COUNT(*) FROM [Products] AS [p]
-- ❌ Sem WHERE clause
```

**Causa**: Este era um bug conhecido (corrigido na v3.2.0) onde `FilterBy` era um `UnaryExpression` ao invés de `ConstantExpression`.

**Solução**:
1. **Atualize para v3.2.0+**: `dotnet add package Nuuvify.CommonPack.UnitOfWork.Abstraction --version 3.2.0`
2. **Verifique sua lista não está vazia**:
   ```csharp
   filter.SearchTerms = new List<string> { "iPhone", "Samsung" }; // ✅ Correto
   filter.SearchTerms = new List<string>(); // ❌ Lista vazia = sem filtro
   filter.SearchTerms = null; // ❌ Null = sem filtro
   ```
3. **Valide o operador**:
   ```csharp
   [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList, HasName = nameof(Product.Name))]
   public List<string>? SearchTerms { get; set; }
   ```

### Problema: PageIndex inconsistente

**Sintoma**: `PageIndex = 1` retorna registros da segunda página.

**Causa**: Confusão entre 0-based e 1-based index (corrigido na v3.2.0).

**Solução**:
- **v3.2.0+**: `PageIndex = 1` é a primeira página (1-based)
- **Versões anteriores**: Use `PageIndex = 0` para primeira página (0-based)

```csharp
// v3.2.0+
filter.PageIndex = 1; // ✅ Primeira página
filter.PageIndex = 2; // ✅ Segunda página

// Versões < 3.2.0
filter.PageIndex = 0; // Primeira página
filter.PageIndex = 1; // Segunda página
```

### Problema: NullReferenceException em Expression.And/Or

**Sintoma**: Exception ao combinar múltiplos filtros.

```
System.NullReferenceException: Object reference not set to an instance of an object
   at System.Linq.Expressions.Expression.And(Expression left, Expression right)
```

**Causa**: Filtros retornando `null` (corrigido na v3.2.0).

**Solução**:
1. **Atualize para v3.2.0+** onde null expressions são automaticamente ignoradas
2. **Valide valores antes de atribuir**:
   ```csharp
   // ✅ BOM - validação
   if (!string.IsNullOrEmpty(searchTerm))
   {
       filter.NameSearch = searchTerm;
   }

   // ❌ EVITAR - pode causar null expression
   filter.NameSearch = ""; // String vazia pode gerar problemas
   ```

### Problema: Case-insensitive não funciona

**Sintoma**: Busca por "iphone" não encontra "iPhone".

**SQL Esperado vs Gerado**:
```sql
-- ✅ Esperado:
WHERE UPPER([p].[Name]) LIKE UPPER('%iphone%')

-- ❌ Gerado (bug):
WHERE [p].[Name] LIKE '%iphone%'
```

**Solução**:
1. **Verifique o atributo**:
   ```csharp
   [QueryOperator(Operator = WhereOperator.Contains,
                  HasName = nameof(Product.Name),
                  CaseSensitive = false)] // ✅ Essencial!
   public string? NameSearch { get; set; }
   ```

2. **Para ContainsWithLikeForList** (v3.2.0+):
   ```csharp
   [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList,
                  HasName = nameof(Product.Name),
                  CaseSensitive = false)] // ✅ Funciona na v3.2.0+
   public List<string>? SearchTerms { get; set; }

   // SQL Gerado: WHERE Name LIKE '%IPHONE%' OR Name LIKE '%SAMSUNG%'
   ```

### Problema: SQL gerado com "WHERE 0 = 1"

**Sintoma**: Query retorna 0 registros mesmo com dados no banco.

**SQL Gerado**:
```sql
SELECT * FROM Products WHERE 0 = 1
```

**Causa**: Bug em listas vazias (corrigido na v3.2.0).

**Solução**:
1. **Atualize para v3.2.0+**
2. **Evite listas vazias**:
   ```csharp
   // ✅ BOM
   if (terms != null && terms.Any())
   {
       filter.SearchTerms = terms;
   }
   // Deixe null se não houver termos

   // ❌ EVITAR em versões < 3.2.0
   filter.SearchTerms = new List<string>(); // Gera WHERE 0 = 1
   ```

### Problema: Performance lenta em queries grandes

**Sintomas**:
- Queries demoram muito tempo
- Alto consumo de memória
- Timeout de banco de dados

**Soluções**:

1. **Use projeção (Select)**:
   ```csharp
   // ✅ BOM - apenas campos necessários
   var products = await _unitOfWork.Repository<Product>()
       .GetAll()
       .Filter(filter)
       .Select(p => new ProductDto
       {
           Id = p.Id,
           Name = p.Name
       })
       .ToListAsync();

   // ❌ EVITAR - carrega entidade completa
   var products = await _unitOfWork.Repository<Product>()
       .GetAll()
       .Filter(filter)
       .ToListAsync();
   ```

2. **Limite o PageSize**:
   ```csharp
   // ✅ BOM
   filter.PageSize = 50; // Máximo 50 registros por página

   // ❌ EVITAR
   filter.PageSize = 10000; // Muito grande!
   ```

3. **Use AsNoTracking para leitura**:
   ```csharp
   var products = await _unitOfWork.Repository<Product>()
       .GetAll(disableTracking: true) // ✅ Melhora performance em queries read-only
       .Filter(filter)
       .ToListAsync();
   ```

4. **Evite Include desnecessário**:
   ```csharp
   // ✅ BOM - Include apenas quando necessário
   var orders = await _unitOfWork.Repository<Order>()
       .GetAll(include: source => source
           .Include(o => o.Items))
       .Filter(filter)
       .ToListAsync();

   // ❌ EVITAR - Includes desnecessários
   var orders = await _unitOfWork.Repository<Order>()
       .GetAll(include: source => source
           .Include(o => o.Items)
           .Include(o => o.Customer)
           .Include(o => o.ShippingAddress)
           .Include(o => o.PaymentDetails)) // Dados não utilizados
       .Filter(filter)
       .ToListAsync();
   ```

### Problema: Ordenação não funciona

**Sintoma**: Registros não ordenados conforme especificado.

**Solução**:

1. **Sintaxe correta**:
   ```csharp
   // ✅ CORRETO - Formato com prefixo D- ou A-
   filter.Sort = "A-Name";                  // Ascendente
   filter.Sort = "D-Price";                 // Descendente
   filter.Sort = "A-Category, D-Price, A-Name"; // Múltiplos campos

   // ❌ INCORRETO - Formato antigo (não suportado)
   filter.Sort = "Name asc";                // ❌ Use "A-Name"
   filter.Sort = "Price desc";              // ❌ Use "D-Price"
   filter.Sort = "Name,Price";              // ❌ Faltam prefixos D- ou A-
   ```

2. **Propriedade existe na entidade**:
   ```csharp
   // ✅ Propriedade existe em Product
   filter.Sort = "A-Name";

   // ❌ Propriedade não existe
   filter.Sort = "D-InvalidProperty"; // Runtime error
   ```

### Recursos de Debug

**Habilitar logging de SQL (EF Core)**:
```csharp
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}

// Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging() // ⚠️ Apenas em desenvolvimento!
           .LogTo(Console.WriteLine, LogLevel.Information);
});
```

**Inspecionar expressões geradas**:
```csharp
var filterExpression = _unitOfWork.Repository<Product>()
    .GetAll()
    .FilterExpression(filter);

Console.WriteLine(filterExpression?.ToString());
// Saída: p => (p.Name.Contains("iPhone") OR p.Name.Contains("Samsung")) AND p.Price >= 100
```

## 📄 Licença

Este projeto está licenciado sob a [Licença MIT](LICENSE).

## 🤝 Contribuição

Contribuições são bem-vindas! Para contribuir:

1. Fork o projeto
2. Crie uma feature branch (`git checkout -b feature/nova-funcionalidade`)
3. Commit suas mudanças (`git commit -am 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/nova-funcionalidade`)
5. Abra um Pull Request

## 📞 Suporte

Para dúvidas e suporte técnico:

- 📧 Email: [suporte@zocate.li](mailto:suporte@zocate.li)
- 📋 Issues: [GitHub Issues](https://github.com/nuuvify/Nuuvify.CommonPack/issues)
- 📖 Documentação: [Wiki do Projeto](https://github.com/nuuvify/Nuuvify.CommonPack/wiki)

## 📈 Versionamento

Este projeto segue o [Semantic Versioning](https://semver.org/):
- **MAJOR**: Mudanças incompatíveis na API
- **MINOR**: Novas funcionalidades mantendo compatibilidade
- **PATCH**: Correções de bugs mantendo compatibilidade

Consulte o [CHANGELOG.md](./CHANGELOG.md) para ver todas as mudanças.

## 🏢 Sobre a Nuuvify

A **Nuuvify** é uma empresa especializada em soluções tecnológicas para transformação digital, oferecendo bibliotecas e ferramentas robustas para acelerar o desenvolvimento de aplicações empresariais.

### Outros Pacotes da CommonPack
- [`Nuuvify.CommonPack.AzureServiceBus`](../Nuuvify.CommonPack.AzureServiceBus/) - Azure Service Bus integration
- [`Nuuvify.CommonPack.Email`](../Nuuvify.CommonPack.Email/) - Biblioteca para envio de emails
- [`Nuuvify.CommonPack.Security`](../Nuuvify.CommonPack.Security/) - Ferramentas de segurança
- [`Nuuvify.CommonPack.Middleware`](../Nuuvify.CommonPack.Middleware/) - Middlewares customizados
- [`Nuuvify.CommonPack.Extensions`](../Nuuvify.CommonPack.Extensions/) - Extensões úteis para .NET

---

Desenvolvido com ❤️ pela equipe **Nuuvify**.
