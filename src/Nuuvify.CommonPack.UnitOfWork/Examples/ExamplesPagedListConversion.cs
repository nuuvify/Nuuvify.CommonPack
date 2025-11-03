using System.Globalization;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Collections;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;

#nullable enable

namespace Nuuvify.CommonPack.UnitOfWork.Examples;

/// <summary>
/// Exemplos de uso do método PagedList.From() para conversão entre tipos.
///
/// Este exemplo demonstra:
/// - ✅ Conversão de IPagedList&lt;TSource&gt; para IPagedList&lt;TResult&gt;
/// - ✅ Mapeamento de entidades para DTOs preservando metadados de paginação
/// - ✅ Transformação de dados após paginação
/// - ✅ Uso de funções de conversão customizadas
/// </summary>
public class ExamplesPagedListConversion
{
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Order> _orderRepository;

    public ExamplesPagedListConversion(
        IRepository<Product> productRepository,
        IRepository<Order> orderRepository)
    {
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    #region Exemplo 1: Conversão Simples de Entity para DTO

    /// <summary>
    /// Exemplo básico: Converter IPagedList&lt;Product&gt; para IPagedList&lt;ProductDto&gt;
    ///
    /// Cenário: Você já tem uma lista paginada de entidades e precisa convertê-la para DTOs
    /// </summary>
    public async Task<IPagedList<ProductDto>> ConvertProductsToDto(int pageIndex = 1, int pageSize = 10)
    {
        // 1. Obter lista paginada de entidades
        var pagedProducts = await _productRepository.GetPagedListAsync(
            predicate: p => p.IsActive,
            orderBy: query => query.OrderBy(p => p.Name),
            pageIndex: pageIndex,
            pageSize: pageSize);

        // 2. Converter usando PagedList.From()
        // ✅ Preserva metadados: PageIndex, PageSize, TotalCount, TotalPages
        var pagedProductsDto = PagedList.From<ProductDto, Product>(
            source: pagedProducts,
            converter: products => products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category,
                Stock = p.Stock
            }));

        return pagedProductsDto;
    }

    #endregion

    #region Exemplo 2: Conversão com Lógica de Negócio

    /// <summary>
    /// Exemplo avançado: Converter com transformações e cálculos
    ///
    /// Cenário: Aplicar lógica de negócio durante a conversão
    /// </summary>
    public async Task<IPagedList<ProductSummaryDto>> ConvertWithBusinessLogic(int pageIndex = 1, int pageSize = 20)
    {
        var pagedProducts = await _productRepository.GetPagedListAsync(
            pageIndex: pageIndex,
            pageSize: pageSize);

        // Conversão com lógica de negócio
        var pagedSummary = PagedList.From<ProductSummaryDto, Product>(
            source: pagedProducts,
            converter: products => products.Select(p => new ProductSummaryDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category,
                // ✅ Lógica de negócio: Status baseado em estoque
                InStock = p.Stock > 0,
                // ✅ Lógica de negócio: Cálculo de dias desde criação
                DaysOld = (DateTime.UtcNow - p.CreatedAt).Days
            }));

        return pagedSummary;
    }

    #endregion

    #region Exemplo 3: Múltiplas Conversões em Pipeline

    /// <summary>
    /// Exemplo de pipeline: Converter múltiplas vezes para diferentes formatos
    ///
    /// Cenário: Transformar dados em múltiplas etapas
    /// </summary>
    public async Task<IPagedList<ProductDisplayDto>> ConvertInPipeline(int pageIndex = 1, int pageSize = 10)
    {
        // Passo 1: Obter entidades paginadas
        var pagedProducts = await _productRepository.GetPagedListAsync(
            pageIndex: pageIndex,
            pageSize: pageSize);

        // Passo 2: Primeira conversão - DTO básico
        var pagedBasicDto = PagedList.From<ProductBasicDto, Product>(
            source: pagedProducts,
            converter: products => products.Select(p => new ProductBasicDto
            {
                Id = p.Id.ToString(CultureInfo.InvariantCulture),
                Name = p.Name,
                Price = p.Price
            }));

        // Passo 3: Segunda conversão - DTO de exibição (enriquecido)
        var pagedDisplayDto = PagedList.From<ProductDisplayDto, ProductBasicDto>(
            source: pagedBasicDto,
            converter: dtos => dtos.Select(dto => new ProductDisplayDto
            {
                Id = dto.Id,
                DisplayName = $"#{dto.Id} - {dto.Name}",
                DisplayPrice = $"R$ {dto.Price:N2}",
                // ✅ Adicionar informações de exibição
                CssClass = dto.Price > 100 ? "high-price" : "normal-price",
                ShowBadge = dto.Price > 500
            }));

        return pagedDisplayDto;
    }

    #endregion

    #region Exemplo 4: Conversão com Agregação de Dados Relacionados

    /// <summary>
    /// Exemplo com agregação: Converter Orders incluindo resumo de itens
    ///
    /// Cenário: Converter entidades complexas com dados relacionados
    /// </summary>
    public async Task<IPagedList<OrderSummaryDto>> ConvertOrdersWithAggregation(int pageIndex = 1, int pageSize = 10)
    {
        var pagedOrders = await _orderRepository.GetPagedListAsync(
            pageIndex: pageIndex,
            pageSize: pageSize);

        var pagedOrderSummary = PagedList.From<OrderSummaryDto, Order>(
            source: pagedOrders,
            converter: orders => orders.Select(o => new OrderSummaryDto
            {
                OrderId = o.Id.ToString(CultureInfo.InvariantCulture),
                OrderDate = o.OrderDate,
                // ✅ Agregação: Total de itens
                TotalItems = o.Items.Count,
                // ✅ Agregação: Valor total calculado
                TotalAmount = o.Items.Sum(i => i.Quantity * i.UnitPrice),
                // ✅ Agregação: Lista de produtos
                ProductNames = string.Join(", ", o.Items.Select(i => i.Product?.Name ?? "N/A").Take(3)),
                // ✅ Agregação: Indicador de muitos itens
                HasManyItems = o.Items.Count > 5
            }));

        return pagedOrderSummary;
    }

    #endregion

    #region Exemplo 5: Conversão para Lista Vazia

    /// <summary>
    /// Exemplo especial: Converter uma lista vazia
    ///
    /// Cenário: Garantir que metadados sejam preservados mesmo sem dados
    /// </summary>
    public IPagedList<ProductDto> ConvertEmptyList()
    {
        // Criar lista vazia usando PagedList.Empty<T>()
        var emptyProducts = PagedList.Empty<Product>();

        // Converter lista vazia - metadados são preservados
        var emptyProductsDto = PagedList.From<ProductDto, Product>(
            source: emptyProducts,
            converter: products => products.Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                Category = p.Category,
                Stock = p.Stock
            }));

        // ✅ Resultado: IPagedList<ProductDto> vazio com metadados corretos
        // PageIndex = 0, PageSize = 0, TotalCount = 0, TotalPages = 0
        return emptyProductsDto;
    }

    #endregion

    #region Exemplo 6: Uso Prático em Controller/Service

    /// <summary>
    /// Exemplo real: Como usar em um serviço típico
    ///
    /// Cenário: Padrão comum em aplicações reais
    /// </summary>
    public async Task<IPagedList<ProductCardDto>> GetProductCardsAsync(
        ProductFilterModel filter)
    {
        // 1. Buscar produtos com filtros dinâmicos
        var query = _productRepository.GetAll()
            .Where(p => p.IsActive);

        // Aplicar filtros opcionais
        if (!string.IsNullOrEmpty(filter.CategoryId))
            query = query.Where(p => p.Category == filter.CategoryId);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.Price >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.Price <= filter.MaxPrice.Value);

        // 2. Paginar
        var pagedProducts = await query
            .OrderBy(p => p.Name)
            .ToPagedListAsync(filter.PageIndex, filter.PageSize);

        // 3. Converter para DTO de exibição usando PagedList.From()
        var pagedCards = PagedList.From<ProductCardDto, Product>(
            source: pagedProducts,
            converter: products => products.Select(p => new ProductCardDto
            {
                Id = p.Id.ToString(CultureInfo.InvariantCulture),
                Title = p.Name,
                Subtitle = p.Category,
                Price = p.Price,
                PriceLabel = $"R$ {p.Price:N2}",
                ImageUrl = $"/images/products/{p.Id}.jpg",
                IsAvailable = p.Stock > 0,
                StockLabel = p.Stock > 0 ? $"{p.Stock} em estoque" : "Esgotado"
            }));

        return pagedCards;
    }

    #endregion
}

#region DTOs Exclusivos para Conversão

/// <summary>DTO básico intermediário</summary>
public class ProductBasicDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
}

/// <summary>DTO para exibição final</summary>
public class ProductDisplayDto
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string DisplayPrice { get; set; } = string.Empty;
    public string CssClass { get; set; } = string.Empty;
    public bool ShowBadge { get; set; }
}

/// <summary>DTO de resumo de pedido</summary>
public class OrderSummaryDto
{
    public string OrderId { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
    public string ProductNames { get; set; } = string.Empty;
    public bool HasManyItems { get; set; }
}

/// <summary>DTO para card de produto</summary>
public class ProductCardDto
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string PriceLabel { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string StockLabel { get; set; } = string.Empty;
}

/// <summary>Modelo de filtro de produto</summary>
public class ProductFilterModel
{
    public string? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

#endregion
