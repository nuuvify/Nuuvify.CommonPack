using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Filter;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

namespace Nuuvify.CommonPack.UnitOfWork.Examples;

public class ExamplesQueryOperators
{
    private readonly ExampleDbContext _exampleDbContext;
    private readonly ILogger<ExamplesQueryOperators> _logger;

    public ExamplesQueryOperators(
        ExampleDbContext exampleDbContext,
        ILogger<ExamplesQueryOperators> logger)
    {
        _exampleDbContext = exampleDbContext;
        _logger = logger;
    }

    public async Task<IPagedList<ProductDto>> GetProductByNameAsync(ProductCaseInsensitiveFilterModel filterModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var products = _exampleDbContext.Products
                .AsQueryable()
                .Filter(filterModel)
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price,
                    Stock = p.Stock
                });

            return await products.ToPagedListAsync(filterModel.PageIndex, filterModel.PageSize, 0, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto: {NameIgnoreCase}", filterModel.NameIgnoreCase);
            throw;
        }
    }

    /// <summary>
    /// Busca produtos com filtro case-insensitive e ordenação por Name e IsActive
    /// </summary>
    public async Task<IPagedList<ProductDto>> GetProductByNameWithSortAsync(ProductCaseInsensitiveFilterModel filterModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var products = _exampleDbContext.Products
                .AsQueryable()
                .Filter(filterModel)
                .Sort("A-Name,A-IsActive")
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price,
                    Stock = p.Stock
                });

            return await products.ToPagedListAsync(filterModel.PageIndex, filterModel.PageSize, 0, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto: {NameIgnoreCase}", filterModel.NameIgnoreCase);
            throw;
        }
    }

    /// <summary>
    /// Busca produtos com filtro case-insensitive e ordenação por Name, IsActive e LastUpdate descendente
    /// </summary>
    public async Task<IPagedList<ProductDto>> GetProductByNameWithMultipleSortAsync(ProductCaseInsensitiveFilterModel filterModel, CancellationToken cancellationToken = default)
    {
        try
        {
            var products = _exampleDbContext.Products
                .AsQueryable()
                .Filter(filterModel)
                .Sort("A-Name,A-IsActive,D-LastUpdate")
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price,
                    Stock = p.Stock
                });

            return await products.ToPagedListAsync(filterModel.PageIndex, filterModel.PageSize, 0, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto: {NameIgnoreCase}", filterModel.NameIgnoreCase);
            throw;
        }
    }

    /// <summary>
    /// Busca avançada com FilterExpression usando múltiplos campos e operadores
    /// Demonstra: range de preços, busca em nome, filtro de categoria, estoque mínimo e ordenação
    /// </summary>
    public async Task<IPagedList<ProductDto>> GetProductsWithComplexFilterAsync(ComplexProductFilterModel filterModel, CancellationToken cancellationToken = default)
    {
        try
        {
            // ✅ Cria a expressão de filtro dinamicamente baseada no modelo
            var filterExpression = _exampleDbContext.Products
                .AsQueryable()
                .FilterExpression(filterModel);

            var query = _exampleDbContext.Products.AsQueryable();

            // ✅ Aplica a expressão apenas se houver filtros
            if (filterExpression != null)
            {
                query = query.Where(filterExpression);
            }

            // ✅ Aplica ordenação: Price descendente, Name ascendente
            var products = query
                .Sort("D-Price,A-Name")
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price,
                    Stock = p.Stock
                });

            _logger.LogInformation(
                "Filtro complexo - Termos: [{GlobalTerms}], Preço: {MinPrice}-{MaxPrice}, Categoria: {Category}, Estoque mínimo: {MinStock}",
                filterModel.GlobalTerms != null ? string.Join(", ", filterModel.GlobalTerms) : "N/A",
                filterModel.MinPrice,
                filterModel.MaxPrice,
                filterModel.PrimaryCategory ?? filterModel.AlternativeCategory ?? "N/A",
                filterModel.MinStock);

            return await products.ToPagedListAsync(filterModel.PageIndex, filterModel.PageSize, 0, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos com filtro complexo");
            throw;
        }
    }

}
