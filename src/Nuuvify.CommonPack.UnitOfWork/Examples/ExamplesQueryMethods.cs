using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

#nullable enable

namespace Nuuvify.CommonPack.UnitOfWork.Examples;

/// <summary>
/// Demonstração simplificada do uso correto de IRepository&lt;T&gt; e IPagedList&lt;T&gt;.
/// 
/// Este exemplo mostra:
/// - ✅ Operações CRUD via IRepository&lt;Product&gt;
/// - ✅ Paginação real com IPagedList&lt;T&gt; interface
/// - ✅ Queries com filtros via Repository ReadOnly operations
/// - ✅ Uso correto do Unit of Work para transações
/// - ✅ Projeções eficientes para DTOs
/// </summary>
public class ExamplesQueryMethods
{
    private readonly IRepository<Product> _productRepository;
    private readonly IUnitOfWork<ExampleDbContext> _unitOfWork;
    private readonly ILogger<ExamplesQueryMethods> _logger;

    public ExamplesQueryMethods(
        IRepository<Product> productRepository,
        IUnitOfWork<ExampleDbContext> unitOfWork,
        ILogger<ExamplesQueryMethods> logger)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Demonstração de IPagedList<T> - Paginação Real

    /// <summary>
    /// Exemplo 1: Paginação básica com IPagedList
    /// Demonstra como usar GetPagedListAsync que retorna IPagedList&lt;T&gt;
    /// </summary>
    public async Task<IPagedList<ProductDto>> GetProductsPaginatedAsync(int pageIndex = 1, int pageSize = 20)
    {
        // ✅ Uso correto do IRepository<Product>.GetPagedListAsync que retorna IPagedList<ProductDto>
        var pagedProducts = await _productRepository.GetPagedListAsync(
            predicate: p => p.IsActive,
            orderBy: query => query.OrderBy(p => p.Name),
            pageIndex: pageIndex,
            pageSize: pageSize,
            selector: p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock
            });

        _logger.LogInformation("Página {PageIndex}/{TotalPages} - {ItemCount}/{TotalCount} produtos",
            pagedProducts.PageIndex, pagedProducts.TotalPages,
            pagedProducts.Items.Count, pagedProducts.TotalCount);

        // ✅ pagedProducts implementa IPagedList<ProductDto> com todas as propriedades:
        // - Items: List<ProductDto>
        // - PageIndex, PageSize, TotalCount, TotalPages
        // - HasNextPage, HasPreviousPage
        // - Skip, Take (calculados automaticamente)

        return pagedProducts;
    }

    /// <summary>
    /// Exemplo 2: Busca com filtros e paginação
    /// Demonstra filtros de texto, preço e paginação com IPagedList
    /// </summary>
    public async Task<IPagedList<ProductDto>> SearchProductsAsync(
        string? searchTerm = null,
        string? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageIndex = 1,
        int pageSize = 10)
    {
        // ✅ IRepository permite queries complexas com Where, OrderBy, etc.
        var pagedProducts = await _productRepository.GetPagedListAsync(
            predicate: p => p.IsActive &&
                           (searchTerm == null || p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm)) &&
                           (category == null || p.Category == category) &&
                           (minPrice == null || p.Price >= minPrice) &&
                           (maxPrice == null || p.Price <= maxPrice),
            orderBy: query => query.OrderBy(p => p.Price).ThenBy(p => p.Name),
            pageIndex: pageIndex,
            pageSize: pageSize,
            selector: p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock
            });

        _logger.LogInformation("Busca '{SearchTerm}' categoria '{Category}' preço {MinPrice}-{MaxPrice}: {Count}/{Total}",
            searchTerm, category, minPrice, maxPrice, pagedProducts.Items.Count, pagedProducts.TotalCount);

        return pagedProducts;
    }

    /// <summary>
    /// Exemplo 3: Busca global com múltiplos termos (OR logic)
    /// Demonstra como fazer busca em múltiplos campos com lógica OR
    /// </summary>
    public async Task<IPagedList<ProductDto>> GlobalSearchAsync(
        string[] searchTerms,
        int pageIndex = 1,
        int pageSize = 15)
    {
        if (searchTerms == null || searchTerms.Length == 0)
        {
            return await GetProductsPaginatedAsync(pageIndex, pageSize);
        }

        // ✅ Busca OR: produtos que contenham QUALQUER um dos termos
        var pagedProducts = await _productRepository.GetPagedListAsync(
            predicate: p => p.IsActive &&
                           searchTerms.Any(term =>
                               p.Name.Contains(term) ||
                               p.Description.Contains(term) ||
                               p.Category.Contains(term)),
            orderBy: query => query.OrderBy(p => p.Name),
            pageIndex: pageIndex,
            pageSize: pageSize,
            selector: p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock
            });

        _logger.LogInformation("Busca global [{Terms}]: {Count}/{Total} - Página {Page}/{TotalPages}",
            string.Join(", ", searchTerms), pagedProducts.Items.Count, pagedProducts.TotalCount,
            pagedProducts.PageIndex, pagedProducts.TotalPages);

        return pagedProducts;
    }

    #endregion

    #region Demonstração de IRepository<T> CRUD Operations

    /// <summary>
    /// Exemplo 4: Criar produto via IRepository
    /// Demonstra operações de criação usando Repository Pattern
    /// </summary>
    public async Task<ProductDto> CreateProductAsync(string name, string description, string category, decimal price)
    {
        var product = new Product
        {
            Name = name,
            Description = description,
            Category = category,
            Price = price,
            Stock = 0,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // ✅ IRepository<Product>.Add - adiciona à unidade de trabalho
        _ = await _productRepository.Add(product);

        // ✅ IUnitOfWork.SaveChangesAsync - confirma a transação
        _ = await _unitOfWork.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            Price = product.Price,
            Stock = product.Stock
        };
    }

    /// <summary>
    /// Exemplo 5: Atualizar produto via IRepository
    /// Demonstra operações de atualização usando Repository Pattern
    /// </summary>
    public async Task<ProductDto?> UpdateProductAsync(int id, string name, decimal price, int stock)
    {
        // ✅ IRepository<Product>.FindAsync - busca por ID
        var product = await _productRepository.FindAsync(id);

        if (product is null)
        {
            return null;
        }

        // Atualizar propriedades
        product.Name = name;
        product.Price = price;
        product.Stock = stock;
        product.LastUpdate = DateTime.UtcNow;

        // ✅ IRepository<Product>.Update - marca como modificado
        _productRepository.Update(product);

        // ✅ Unit of Work confirma a transação
        _ = await _unitOfWork.SaveChangesAsync();

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Category = product.Category,
            Price = product.Price,
            Stock = product.Stock
        };
    }

    /// <summary>
    /// Exemplo 6: Soft Delete via IRepository
    /// Demonstra exclusão lógica usando Repository Pattern
    /// </summary>
    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _productRepository.FindAsync(id);

        if (product is null)
        {
            return false;
        }

        // Soft delete - marca como inativo
        product.IsActive = false;
        product.LastUpdate = DateTime.UtcNow;

        _productRepository.Update(product);
        _ = await _unitOfWork.SaveChangesAsync();

        return true;
    }

    #endregion

    #region Demonstração de Queries ReadOnly

    /// <summary>
    /// Exemplo 7: Queries readonly via IRepository
    /// Demonstra como usar GetAll() com ordenação múltipla e descendente
    /// </summary>
    public async Task<List<ProductDto>> GetProductsByCategoryAsync(string category)
    {
        // ✅ IRepository.GetAll() retorna IQueryable para queries readonly
        // Ordenação: IsActive descendente, depois Price descendente, depois Name ascendente
        var products = await _productRepository.GetAll()
            .Where(p => p.Category == category && p.IsActive)
            .OrderByDescending(p => p.IsActive)
            .ThenByDescending(p => p.Price)
            .ThenBy(p => p.Name)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock
            })
            .ToListAsync();

        return products;
    }

    /// <summary>
    /// Exemplo 8: GetFirstOrDefault via IRepository
    /// Demonstra busca de item único usando Repository ReadOnly
    /// </summary>
    public async Task<ProductDto?> GetProductByNameAsync(string name)
    {
        // ✅ IRepository.GetFirstOrDefaultAsync com predicate e selector
        var productDto = await _productRepository.GetFirstOrDefaultAsync(
            predicate: p => p.Name == name && p.IsActive,
            selector: p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Category = p.Category,
                Price = p.Price,
                Stock = p.Stock
            });

        return productDto;
    }

    #endregion

    #region Demonstração de Estatísticas e Agregações

    /// <summary>
    /// Exemplo 9: Estatísticas usando IRepository ReadOnly
    /// Demonstra agregações e group by usando Repository Pattern
    /// </summary>
    public async Task<ProductStatsDto> GetProductStatisticsAsync()
    {
        // ✅ Usar GetAll() para queries de agregação
        var products = _productRepository.GetAll().Where(p => p.IsActive);

        var stats = new ProductStatsDto
        {
            TotalProducts = await products.CountAsync(),
            AveragePrice = await products.AverageAsync(p => p.Price),
            MinPrice = await products.MinAsync(p => p.Price),
            MaxPrice = await products.MaxAsync(p => p.Price),
            TotalStock = await products.SumAsync(p => p.Stock),

            // Group by categoria
            CategoryStats = await products
                .GroupBy(p => p.Category)
                .Select(g => new CategoryStatsDto
                {
                    Category = g.Key,
                    Count = g.Count(),
                    AveragePrice = g.Average(p => p.Price),
                    TotalStock = g.Sum(p => p.Stock)
                })
                .OrderByDescending(c => c.Count)
                .ToListAsync()
        };

        return stats;
    }

    #endregion
}
