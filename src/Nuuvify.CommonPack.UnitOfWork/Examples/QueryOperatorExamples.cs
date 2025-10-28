using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.UnitOfWork.Abstraction;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;
using Product = Nuuvify.CommonPack.UnitOfWork.Examples.Shared.Product;
using ExampleDbContext = Nuuvify.CommonPack.UnitOfWork.Examples.Shared.ExampleDbContext;

namespace Nuuvify.CommonPack.UnitOfWork.Examples;

/// <summary>
/// Demonstração completa de todos os 13 operadores de filtro disponíveis usando Repository Pattern.
/// 
/// Este exemplo inclui:
/// - ✅ 12 operadores básicos de comparação e texto
/// - ✅ 🆕 Operador ContainsWithLikeForList (busca OR múltipla)
/// - ✅ Modificadores: CaseSensitive, UseOr, UseNot
/// - ✅ Casos de uso reais para cada operador
/// - ✅ Combinações complexas de filtros
/// - ✅ Paginação com IPagedList<> interface
/// </summary>
public class QueryOperatorExamples
{
    private readonly IRepository<Product> _productRepository;
    private readonly ILogger<QueryOperatorExamples> _logger;

    public QueryOperatorExamples(
        IRepository<Product> productRepository,
        ILogger<QueryOperatorExamples> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    #region 1. Operadores de Igualdade

    /// <summary>
    /// Exemplo 1: Operador EQUALS com paginação via IPagedList
    /// Busca por igualdade exata - ideal para IDs, status, categorias
    /// </summary>
    public async Task<IPagedList<ProductCatalog>> GetProductsByExactCategoryAsync(string category, int pageIndex = 1, int pageSize = 20)
    {
        var filter = new ProductEqualsFilterModel
        {
            CategoryExact = category,
            IsActive = true, // Filtro adicional
            PageIndex = pageIndex.ToString(),
            PageSize = pageSize.ToString()
        };

        // SQL gerado: WHERE Category = @category AND IsActive = @isActive
        var result = await _productRepository.GetPagedListAsync(
            predicate: filter.BuildPredicate<Product>(),
            orderBy: query => query.OrderBy(p => p.Name),
            pageIndex: pageIndex,
            pageSize: pageSize,
            selector: p => new ProductCatalog
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price
            });

        _logger.LogInformation("Produtos encontrados por categoria '{Category}': {Count}/{Total}",
            category, result.Items.Count, result.TotalCount);

        return result;
    }

    /// <summary>
    /// Exemplo 2: Operador NOT EQUALS com IPagedList
    /// Exclusão de valores específicos
    /// </summary>
    public async Task<IPagedList<ProductCatalog>> GetProductsExcludingCategoryAsync(string excludeCategory, int pageIndex = 1, int pageSize = 20)
    {
        // SQL gerado: WHERE Category <> @excludeCategory
        var result = await _productRepository.GetPagedListAsync(
            predicate: p => p.Category != excludeCategory,
            orderBy: query => query.OrderBy(p => p.Name),
            pageIndex: pageIndex,
            pageSize: pageSize,
            selector: p => new ProductCatalog
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price
            });

        _logger.LogInformation("Produtos encontrados excluindo categoria '{ExcludeCategory}': {Count}/{Total}",
            excludeCategory, result.Items.Count, result.TotalCount);

        return result;
    }

    #endregion

    #region 2. Operadores de Comparação Numérica

    /// <summary>
    /// Exemplo 3: Operadores GREATER THAN e LESS THAN com IPagedList
    /// Filtros de faixa de preços usando Repository Pattern
    /// </summary>
    public async Task<IPagedList<ProductCatalog>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice, int pageIndex = 1, int pageSize = 25)
    {
        // SQL gerado: WHERE Price >= @minPrice AND Price <= @maxPrice AND IsActive = true ORDER BY Price ASC
        var result = await _productRepository.GetPagedListAsync(
            predicate: p => p.Price >= minPrice && p.Price <= maxPrice && p.IsActive,
            orderBy: query => query.OrderBy(p => p.Price),
            pageIndex: pageIndex,
            pageSize: pageSize,
            selector: p => new ProductCatalog
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock
            });

        _logger.LogInformation("Produtos na faixa de preço {MinPrice:C} - {MaxPrice:C}: {Count}/{Total}",
            minPrice, maxPrice, result.Items.Count, result.TotalCount);

        return result;
    }

    /// <summary>
    /// Exemplo 4: Operadores GREATER THAN e LESS THAN (sem igualdade)
    /// Filtros exclusivos (não incluem os valores limite)
    /// </summary>
    public async Task<List<ProductCatalog>> GetProductsInExclusivePriceRangeAsync(decimal minPrice, decimal maxPrice)
    {
        var filter = new ProductExclusivePriceFilterModel
        {
            PriceGreaterThan = minPrice,  // GreaterThan (sem igualdade)
            PriceLessThan = maxPrice,     // LessThan (sem igualdade)
            Sort = "Price desc"
        };

        // SQL gerado: WHERE Price > @minPrice AND Price < @maxPrice
        var result = await _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(filter)
            .Select(p => new ProductCatalog { Id = p.Id, Name = p.Name, Price = p.Price })
            .ToPagedResultAsync();

        return result.Items;
    }

    #endregion

    #region 3. Operadores para Campos Nullable

    /// <summary>
    /// Exemplo 5: Operadores NULLABLE-SAFE
    /// Filtros que lidam com campos que podem ser null
    /// </summary>
    public async Task<List<ProductCatalog>> GetProductsByNullableFieldsAsync(DateTime? lastUpdateDate, int? minStock)
    {
        var filter = new ProductNullableFilterModel
        {
            LastUpdateDate = lastUpdateDate,    // EqualsWhenNullable
            MinStock = minStock,                // GreaterThanOrEqualWhenNullable
            PageSize = "15"
        };

        // SQL gerado: 
        // WHERE (LastUpdate = @lastUpdateDate OR @lastUpdateDate IS NULL)
        // AND (Stock >= @minStock OR @minStock IS NULL)
        var result = await _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(filter)
            .Select(p => new ProductCatalog
            {
                Id = p.Id,
                Name = p.Name,
                Stock = p.Stock,
                LastUpdate = p.LastUpdate
            })
            .ToPagedResultAsync();

        return result.Items;
    }

    #endregion

    #region 4. Operadores de Texto

    /// <summary>
    /// Exemplo 6: Operador CONTAINS
    /// Busca de texto que contém uma substring
    /// </summary>
    public async Task<List<ProductCatalog>> SearchProductsByNameAsync(string searchTerm, bool caseSensitive = false)
    {
        var filter = new ProductTextSearchFilterModel
        {
            NameSearch = searchTerm,
            Sort = "Name asc",
            PageSize = "30"
        };

        // Com CaseSensitive = false (padrão no modelo):
        // SQL gerado: WHERE UPPER(Name).Contains(UPPER(@searchTerm))
        var result = await _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(filter)
            .Select(p => new ProductCatalog { Id = p.Id, Name = p.Name, Category = p.Category })
            .ToPagedResultAsync();

        _logger.LogInformation("Busca por '{SearchTerm}': {Count} produtos encontrados",
            searchTerm, result.Items.Count);

        return result.Items;
    }

    /// <summary>
    /// Exemplo 7: Operador STARTS WITH
    /// Busca por prefixo - ideal para códigos, SKUs, etc.
    /// </summary>
    public async Task<List<ProductCatalog>> GetProductsByNamePrefixAsync(string prefix)
    {
        var filter = new ProductPrefixFilterModel
        {
            NameStartsWith = prefix,
            Sort = "Name asc"
        };

        // SQL gerado: WHERE Name.StartsWith(@prefix)
        var result = await _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(filter)
            .Select(p => new ProductCatalog { Id = p.Id, Name = p.Name })
            .ToPagedResultAsync();

        return result.Items;
    }

    #endregion

    #region 5. 🆕 Operador ContainsWithLikeForList (NOVO!)

    /// <summary>
    /// Exemplo 8: 🆕 Busca global com múltiplos termos usando IPagedList
    /// Demonstra busca OR em múltiplos termos - o mais poderoso!
    /// 
    /// Este exemplo mostra como implementar:
    /// ✅ Busca global em múltiplos campos
    /// ✅ Lógica OR para múltiplos termos
    /// ✅ Paginação com metadados completos
    /// ✅ Ordenação customizada
    /// </summary>
    public async Task<IPagedList<ProductCatalog>> GlobalSearchProductsAsync(string[] searchTerms, int pageIndex = 1, int pageSize = 20)
    {
        // Exemplo de busca OR: produtos que contenham QUALQUER um dos termos
        // SQL gerado: WHERE (Name.Contains(@term1) OR Name.Contains(@term2) OR Description.Contains(@term1) OR ...)
        var result = await _productRepository.GetPagedListAsync(
            predicate: p => searchTerms.Any(term =>
                p.Name.Contains(term) ||
                p.Description.Contains(term) ||
                p.Category.Contains(term)) && p.IsActive,
            orderBy: query => query.OrderBy(p => p.Name),
            pageIndex: pageIndex,
            pageSize: pageSize,
            selector: p => new ProductCatalog
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price
            });

        _logger.LogInformation("Busca global com termos [{Terms}]: {Count}/{Total} produtos - Página {Page}/{TotalPages}",
            string.Join(", ", searchTerms), result.Items.Count, result.TotalCount, result.PageIndex, result.TotalPages);

        return result;
    }

    /// <summary>
    /// Exemplo 9: Busca múltipla em categorias
    /// Usando ContainsWithLikeForList para filtrar múltiplas categorias
    /// </summary>
    public async Task<List<ProductCatalog>> GetProductsByMultipleCategoriesAsync(List<string> categories)
    {
        var filter = new ProductMultipleCategoriesFilterModel
        {
            Categories = categories, // OR entre categorias
            Sort = "Category asc, Name asc"
        };

        // SQL: WHERE (Category.Contains(@cat1) OR Category.Contains(@cat2) OR ...)
        var result = await _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(filter)
            .Select(p => new ProductCatalog { Id = p.Id, Name = p.Name, Category = p.Category })
            .ToPagedResultAsync();

        return result.Items;
    }

    #endregion

    #region 6. Modificadores de Comportamento (OR, NOT, CaseSensitive)

    /// <summary>
    /// Exemplo 10: Uso de OR para combinações lógicas
    /// Produtos premium OU em promoção
    /// </summary>
    public async Task<List<ProductCatalog>> GetPremiumOrPromotionalProductsAsync(decimal premiumPrice, string promoCategory)
    {
        var filter = new ProductLogicalFilterModel
        {
            // Filtro principal: preços premium
            PremiumPrice = premiumPrice,        // GreaterThanOrEqualTo

            // Filtro alternativo com OR: categoria promocional
            PromotionalCategory = promoCategory, // Equals com UseOr = true

            Sort = "Price desc"
        };

        // SQL gerado: WHERE (Price >= @premiumPrice) OR (Category = @promoCategory)
        var result = await _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(filter)
            .Select(p => new ProductCatalog
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price
            })
            .ToPagedResultAsync();

        return result.Items;
    }

    /// <summary>
    /// Exemplo 11: Uso de NOT para exclusões
    /// Todos os produtos EXCETO os inativos
    /// </summary>
    public async Task<List<ProductCatalog>> GetAllProductsExceptInactiveAsync()
    {
        var filter = new ProductNotFilterModel
        {
            // UseNot = true inverte a lógica
            ExcludeInactive = false, // NOT (IsActive = false) = apenas ativos
            Sort = "Name asc"
        };

        // SQL gerado: WHERE NOT (IsActive = @excludeInactive)
        // Com excludeInactive = false: NOT (IsActive = false) = produtos ativos
        var result = await _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(filter)
            .Select(p => new ProductCatalog { Id = p.Id, Name = p.Name, Category = p.Category })
            .ToPagedResultAsync();

        return result.Items;
    }

    /// <summary>
    /// Exemplo 12: Case sensitivity demonstração
    /// Busca com e sem sensibilidade a maiúsculas/minúsculas
    /// </summary>
    public async Task<(List<ProductCatalog> CaseSensitive, List<ProductCatalog> CaseInsensitive)>
        CompareCaseSensitivityAsync(string searchTerm)
    {
        // Busca case-sensitive
        var caseSensitiveFilter = new ProductCaseSensitiveFilterModel
        {
            NameExact = searchTerm, // CaseSensitive = true
            PageSize = "10"
        };

        // Busca case-insensitive  
        var caseInsensitiveFilter = new ProductCaseInsensitiveFilterModel
        {
            NameIgnoreCase = searchTerm, // CaseSensitive = false
            PageSize = "10"
        };

        // Executar ambas as buscas
        var caseSensitiveTask = _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(caseSensitiveFilter)
            .Select(p => new ProductCatalog { Id = p.Id, Name = p.Name })
            .ToPagedResultAsync();

        var caseInsensitiveTask = _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(caseInsensitiveFilter)
            .Select(p => new ProductCatalog { Id = p.Id, Name = p.Name })
            .ToPagedResultAsync();

        var results = await Task.WhenAll(caseSensitiveTask, caseInsensitiveTask);

        _logger.LogInformation("Busca '{Term}' - Case sensitive: {Sensitive}, Case insensitive: {Insensitive}",
            searchTerm, results[0].Items.Count, results[1].Items.Count);

        return (results[0].Items, results[1].Items);
    }

    #endregion

    #region 7. Filtros Combinados Complexos

    /// <summary>
    /// Exemplo 13: Filtro super complexo combinando múltiplos operadores
    /// Demonstra a flexibilidade da biblioteca
    /// </summary>
    public async Task<List<ProductCatalog>> AdvancedSearchAsync(ComplexProductSearchModel searchCriteria)
    {
        // Este modelo demonstra uso simultâneo de vários operadores
        var filter = new ComplexProductFilterModel
        {
            // Busca global (ContainsWithLikeForList)
            GlobalTerms = searchCriteria.SearchTerms,

            // Range de preços (GreaterThanOrEqualTo + LessThanOrEqualTo)
            MinPrice = searchCriteria.MinPrice,
            MaxPrice = searchCriteria.MaxPrice,

            // Categoria exata (Equals)
            PrimaryCategory = searchCriteria.Category,

            // Categoria alternativa com OR (Equals + UseOr)
            AlternativeCategory = searchCriteria.AlternativeCategory,

            // Exclusão (NotEquals)
            ExcludeCategory = searchCriteria.ExcludeCategory,

            // Estoque mínimo (GreaterThanOrEqualWhenNullable)
            MinStock = searchCriteria.MinStock,

            // Exclusão de inativos (Equals + UseNot)
            ExcludeInactive = false, // NOT (IsActive = false) = só ativos
            Sort = searchCriteria.Sort ?? "Name asc",
            PageIndex = searchCriteria.PageIndex?.ToString(CultureInfo.InvariantCulture) ?? "1",
            PageSize = searchCriteria.PageSize?.ToString(CultureInfo.InvariantCulture) ?? "20"
        };

        var result = await _unitOfWork.QueryAsync<Product>()
            .FilterAndPaginate(filter)
            .Select(p => new ProductCatalog
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock
            })
            .ToPagedResultAsync();

        _logger.LogInformation("Busca avançada executada: {Count} produtos encontrados", result.Items.Count);

        return result.Items;
    }

    #endregion
}

#region Modelos de Filtro para Cada Operador

/// <summary>
/// Modelo demonstrando operador EQUALS
/// </summary>
public class ProductEqualsFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Shared.Product.Category))]
    public string CategoryExact { get; set; } = string.Empty;

    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Shared.Product.IsActive))]
    public bool? IsActive { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando operador NOT EQUALS
/// </summary>
public class ProductNotEqualsFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.NotEquals, HasName = nameof(Shared.Product.Category))]
    public string ExcludeCategory { get; set; } = string.Empty;

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando operadores GREATER_THAN_OR_EQUAL_TO e LESS_THAN_OR_EQUAL_TO
/// </summary>
public class ProductPriceRangeFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualTo, HasName = nameof(Product.Price))]
    public decimal? MinPrice { get; set; }

    [QueryOperator(Operator = WhereOperator.LessThanOrEqualTo, HasName = nameof(Product.Price))]
    public decimal? MaxPrice { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando operadores GREATER_THAN e LESS_THAN (exclusivos)
/// </summary>
public class ProductExclusivePriceFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.GreaterThan, HasName = nameof(Product.Price))]
    public decimal? PriceGreaterThan { get; set; }

    [QueryOperator(Operator = WhereOperator.LessThan, HasName = nameof(Product.Price))]
    public decimal? PriceLessThan { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando operadores nullable-safe
/// </summary>
public class ProductNullableFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.EqualsWhenNullable, HasName = nameof(Product.LastUpdate))]
    public DateTime? LastUpdateDate { get; set; }

    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualWhenNullable, HasName = nameof(Product.Stock))]
    public int? MinStock { get; set; }

    [QueryOperator(Operator = WhereOperator.LessThanOrEqualWhenNullable, HasName = nameof(Product.Stock))]
    public int? MaxStock { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando operador CONTAINS (case-insensitive)
/// </summary>
public class ProductTextSearchFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Contains, HasName = nameof(Product.Name), CaseSensitive = false)]
    public string? NameSearch { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando operador STARTS_WITH
/// </summary>
public class ProductPrefixFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.StartsWith, HasName = nameof(Product.Name), CaseSensitive = false)]
    public string? NameStartsWith { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// 🆕 Modelo demonstrando operador CONTAINS_WITH_LIKE_FOR_LIST
/// </summary>
public class ProductGlobalSearchFilterModel : IQueryableCustom
{
    /// <summary>
    /// 🆕 Busca OR em múltiplos termos
    /// Exemplo: ["iPhone", "Samsung"] = WHERE (Name.Contains('iPhone') OR Name.Contains('Samsung'))
    /// </summary>
    [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList, HasName = nameof(Product.Name))]
    public List<string>? GlobalSearchTerms { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo para busca em múltiplas categorias usando ContainsWithLikeForList
/// </summary>
public class ProductMultipleCategoriesFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList, HasName = nameof(Product.Category))]
    public List<string>? Categories { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando uso de OR (UseOr = true)
/// </summary>
public class ProductLogicalFilterModel : IQueryableCustom
{
    // Filtro principal
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualTo, HasName = nameof(Product.Price))]
    public decimal? PremiumPrice { get; set; }

    // Filtro alternativo com OR
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Category), UseOr = true)]
    public string? PromotionalCategory { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando uso de NOT (UseNot = true)
/// </summary>
public class ProductNotFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.IsActive), UseNot = true)]
    public bool? ExcludeInactive { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo com case-sensitive = true
/// </summary>
public class ProductCaseSensitiveFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Contains, HasName = nameof(Product.Name), CaseSensitive = true)]
    public string? NameExact { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo com case-sensitive = false
/// </summary>
public class ProductCaseInsensitiveFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Contains, HasName = nameof(Product.Name), CaseSensitive = false)]
    public string? NameIgnoreCase { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo complexo combinando múltiplos operadores
/// </summary>
public class ComplexProductFilterModel : IQueryableCustom
{
    // 🆕 Busca global
    [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList, HasName = nameof(Product.Name))]
    public List<string>? GlobalTerms { get; set; }

    // Range de preços
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualTo, HasName = nameof(Product.Price))]
    public decimal? MinPrice { get; set; }

    [QueryOperator(Operator = WhereOperator.LessThanOrEqualTo, HasName = nameof(Product.Price))]
    public decimal? MaxPrice { get; set; }

    // Categoria principal
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Category))]
    public string? PrimaryCategory { get; set; }

    // Categoria alternativa (OR)
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Category), UseOr = true)]
    public string? AlternativeCategory { get; set; }

    // Exclusão de categoria
    [QueryOperator(Operator = WhereOperator.NotEquals, HasName = nameof(Product.Category))]
    public string? ExcludeCategory { get; set; }

    // Estoque mínimo (nullable-safe)
    [QueryOperator(Operator = WhereOperator.GreaterThanOrEqualWhenNullable, HasName = nameof(Product.Stock))]
    public int? MinStock { get; set; }

    // Exclusão de inativos (NOT)
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.IsActive), UseNot = true)]
    public bool? ExcludeInactive { get; set; }

    public string PageIndex { get; set; } = "1";
    public string PageSize { get; set; } = "20";
    public string Sort { get; set; } = string.Empty;
}

#endregion

#region Request Models

/// <summary>
/// Modelo de entrada para busca avançada
/// </summary>
public class ComplexProductSearchModel
{
    public List<string>? SearchTerms { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Category { get; set; }
    public string? AlternativeCategory { get; set; }
    public string? ExcludeCategory { get; set; }
    public int? MinStock { get; set; }
    public string? Sort { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
}

#endregion

#region DTOs (reutilizando do outro arquivo)

public class ProductCatalog
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime? LastUpdate { get; set; }
}

#endregion
