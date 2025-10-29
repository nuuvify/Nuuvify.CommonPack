using System.Collections.ObjectModel;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Helpers;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.Examples;

#region Modelos de Filtro para Cada Operador

/// <summary>
/// Modelo demonstrando operador EQUALS
/// </summary>
public class ProductEqualsFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.Category))]
    public string CategoryExact { get; set; } = string.Empty;

    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.IsActive))]
    public bool? IsActive { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando operador NOT EQUALS
/// </summary>
public class ProductNotEqualsFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.NotEquals, HasName = nameof(Product.Category))]
    public string ExcludeCategory { get; set; } = string.Empty;

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
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

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
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

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
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

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando operador CONTAINS (case-insensitive)
/// </summary>
public class ProductTextSearchFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Contains, HasName = nameof(Product.Name), CaseSensitive = false)]
    public string? NameSearch { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando operador STARTS_WITH
/// </summary>
public class ProductPrefixFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.StartsWith, HasName = nameof(Product.Name), CaseSensitive = false)]
    public string? NameStartsWith { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
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

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo para busca em múltiplas categorias usando ContainsWithLikeForList
/// </summary>
public class ProductMultipleCategoriesFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.ContainsWithLikeForList, HasName = nameof(Product.Category))]
    public Collection<string>? Categories { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
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

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo demonstrando uso de NOT (UseNot = true)
/// </summary>
public class ProductNotFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Equals, HasName = nameof(Product.IsActive), UseNot = true)]
    public bool? ExcludeInactive { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo com case-sensitive = true
/// </summary>
public class ProductCaseSensitiveFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Contains, HasName = nameof(Product.Name), CaseSensitive = true)]
    public string? NameExact { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo com case-sensitive = false
/// </summary>
public class ProductCaseInsensitiveFilterModel : IQueryableCustom
{
    [QueryOperator(Operator = WhereOperator.Contains, HasName = nameof(Product.Name), CaseSensitive = false)]
    public string? NameIgnoreCase { get; set; }

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = string.Empty;
}

/// <summary>
/// Modelo complexo combinando múltiplos operadores
/// </summary>
public class ComplexProductFilterModel : IQueryableCustom
{
    // Busca global
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

    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string Sort { get; set; } = string.Empty;
}

#endregion

