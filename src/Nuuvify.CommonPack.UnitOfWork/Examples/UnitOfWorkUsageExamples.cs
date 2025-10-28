using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;
using Product = Nuuvify.CommonPack.UnitOfWork.Examples.Shared.Product;
using Order = Nuuvify.CommonPack.UnitOfWork.Examples.Shared.Order;
using OrderItem = Nuuvify.CommonPack.UnitOfWork.Examples.Shared.OrderItem;
using Customer = Nuuvify.CommonPack.UnitOfWork.Examples.Shared.Customer;
using OrderStatus = Nuuvify.CommonPack.UnitOfWork.Examples.Shared.OrderStatus;
using ExampleDbContext = Nuuvify.CommonPack.UnitOfWork.Examples.Shared.ExampleDbContext;

namespace Nuuvify.CommonPack.UnitOfWork.Examples;

/// <summary>
/// Demonstra o uso completo do padrão Unit of Work com Repository Pattern para operações CRUD.
/// 
/// Este exemplo inclui:
/// - Configuração e setup inicial
/// - Operações CRUD via IRepository (Create, Read, Update, Delete)
/// - Queries com paginação via IPagedList
/// - Transações automáticas
/// - Queries com relacionamentos (Include)
/// - Projeções para DTOs
/// - Tratamento de erros
/// </summary>
public class UnitOfWorkUsageExamples
{
    private readonly IUnitOfWork<ExampleDbContext> _unitOfWork;
    private readonly IRepository<Product> _productRepository;
    private readonly IRepository<Order> _orderRepository;
    private readonly IRepository<Customer> _customerRepository;
    private readonly ILogger<UnitOfWorkUsageExamples> _logger;

    public UnitOfWorkUsageExamples(
        IUnitOfWork<ExampleDbContext> unitOfWork,
        IRepository<Product> productRepository,
        IRepository<Order> orderRepository,
        IRepository<Customer> customerRepository,
        ILogger<UnitOfWorkUsageExamples> logger)
    {
        _unitOfWork = unitOfWork;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    #region 1. Operações CRUD Básicas

    /// <summary>
    /// Exemplo 1: Criar um novo produto
    /// Demonstra o padrão básico de criação com Repository Pattern
    /// </summary>
    public async Task<ProductDto> CreateProductAsync(string name, string category, decimal price)
    {
        try
        {
            // ✅ Criar nova entidade
            var product = new Product
            {
                Name = name,
                Category = category,
                Price = price,
                Stock = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // ✅ Adicionar via IRepository<Product>
            await _productRepository.Add(product);

            // ✅ Salvar mudanças através do Unit of Work (transação automática)
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Produto criado com sucesso: {ProductId} - {ProductName}",
                product.Id, product.Name);

            // ✅ Retornar DTO ao invés da entidade
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price,
                Stock = product.Stock
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar produto: {ProductName}", name);
            throw;
        }
    }

    /// <summary>
    /// Exemplo 2: Buscar produto por ID
    /// Demonstra busca simples com conversão para DTO via Repository
    /// </summary>
    public async Task<ProductDto> GetProductByIdAsync(int id)
    {
        try
        {
            // ✅ Buscar por ID usando IRepository<Product>
            var product = await _productRepository.FindAsync(id);

            if (product is null)
            {
                _logger.LogWarning("Produto não encontrado: {ProductId}", id);
                return null;
            }

            // ✅ Converter para DTO
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price,
                Stock = product.Stock
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produto: {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Exemplo 3: Atualizar produto existente
    /// Demonstra update com validação e logging via Repository
    /// </summary>
    public async Task<ProductDto> UpdateProductAsync(int id, string name, decimal price, int stock)
    {
        try
        {
            // ✅ Buscar produto existente via Repository
            var product = await _productRepository.FindAsync(id);

            if (product is null)
            {
                _logger.LogWarning("Produto não encontrado para atualização: {ProductId}", id);
                return null;
            }

            // ✅ Atualizar propriedades
            product.Name = name;
            product.Price = price;
            product.Stock = stock;
            product.LastUpdate = DateTime.UtcNow;

            // ✅ Marcar como modificado via Repository
            _productRepository.Update(product);

            // ✅ Salvar mudanças através do Unit of Work
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Produto atualizado: {ProductId} - {ProductName}",
                product.Id, product.Name);

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category,
                Price = product.Price,
                Stock = product.Stock
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar produto: {ProductId}", id);
            throw;
        }
    }

    /// <summary>
    /// Exemplo 4: Deletar produto (Soft Delete)
    /// Demonstra exclusão lógica ao invés de física via Repository
    /// </summary>
    public async Task<bool> DeleteProductAsync(int id)
    {
        try
        {
            var product = await _productRepository.FindAsync(id);

            if (product is null)
            {
                _logger.LogWarning("Produto não encontrado para exclusão: {ProductId}", id);
                return false;
            }

            // ✅ Soft Delete - marcar como inativo ao invés de deletar
            product.IsActive = false;
            product.LastUpdate = DateTime.UtcNow;

            _productRepository.Update(product);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Produto marcado como inativo: {ProductId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar produto: {ProductId}", id);
            throw;
        }
    }

    #endregion

    #region 2. Queries Avançadas com Relacionamentos

    /// <summary>
    /// Exemplo 5: Query com Include (relacionamentos) usando Repository ReadOnly
    /// Demonstra como carregar entidades relacionadas de forma eficiente
    /// </summary>
    public async Task<List<OrderDto>> GetOrdersWithItemsAsync(int customerId)
    {
        try
        {
            // ✅ Query com Include usando Repository para operações de leitura
            var orders = await _orderRepository.GetAll()
                .Include(o => o.Items)
                    .ThenInclude(i => i.Product) // Include aninhado
                .Where(o => o.CustomerId == customerId && o.IsActive)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // ✅ Projetar para DTO com dados dos relacionamentos
            var orderDtos = orders.Select(order => new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                Items = order.Items.Select(item => new OrderItemDto
                {
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? "N/A",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                }).ToList()
            }).ToList();

            _logger.LogInformation("Encontrados {Count} pedidos para cliente {CustomerId}",
                orderDtos.Count, customerId);

            return orderDtos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar pedidos do cliente: {CustomerId}", customerId);
            throw;
        }
    }

    /// <summary>
    /// Exemplo 6: Projeção direta para DTO (mais eficiente) via Repository
    /// Evita carregar entidades completas quando não necessário
    /// </summary>
    public async Task<List<ProductSummaryDto>> GetProductSummariesAsync()
    {
        try
        {
            // ✅ Projeção direta no banco usando Repository - mais eficiente
            var summaries = await _productRepository.GetAll()
                .Where(p => p.IsActive)
                .Select(p => new ProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Category = p.Category,
                    Price = p.Price,
                    InStock = p.Stock > 0,
                    DaysOld = (DateTime.UtcNow - p.CreatedAt).Days
                })
                .OrderBy(p => p.Category)
                .ThenBy(p => p.Name)
                .ToListAsync();

            _logger.LogInformation("Carregados {Count} resumos de produtos", summaries.Count);
            return summaries;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar resumos de produtos");
            throw;
        }
    }

    #endregion

    #region 3. Transações Complexas

    /// <summary>
    /// Exemplo 7: Transação complexa - Criar pedido com múltiplos itens
    /// Demonstra como o Unit of Work gerencia transações automáticas com múltiplos Repositories
    /// </summary>
    public async Task<OrderDto> CreateOrderWithItemsAsync(int customerId, IEnumerable<OrderItemRequest> itemRequests)
    {
        try
        {
            // ✅ Buscar cliente via Repository
            var customer = await _customerRepository.FindAsync(customerId);
            if (customer is null)
                throw new ArgumentException($"Cliente não encontrado: {customerId}");

            // ✅ Criar novo pedido
            var order = new Order
            {
                CustomerId = customerId,
                CustomerName = customer.Name,
                CustomerEmail = customer.Email,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                IsActive = true
            };

            await _orderRepository.Add(order);

            // ✅ Processar itens do pedido (múltiplas operações na mesma transação)
            decimal totalAmount = 0;

            foreach (var itemRequest in itemRequests)
            {
                // Buscar produto e validar estoque via Repository
                var product = await _productRepository.FindAsync(itemRequest.ProductId);
                if (product is null)
                    throw new ArgumentException($"Produto não encontrado: {itemRequest.ProductId}");

                if (product.Stock < itemRequest.Quantity)
                    throw new InvalidOperationException(
                        $"Estoque insuficiente para produto {product.Name}. " +
                        $"Disponível: {product.Stock}, Solicitado: {itemRequest.Quantity}");

                // Criar item do pedido
                var orderItem = new OrderItem
                {
                    Order = order,
                    ProductId = itemRequest.ProductId,
                    Quantity = itemRequest.Quantity,
                    UnitPrice = product.Price
                };

                // Adicionar via Repository (não temos Repository para OrderItem neste exemplo)
                // Em casos reais, você teria um IRepository<OrderItem>
                order.Items.Add(orderItem);

                // Atualizar estoque do produto via Repository
                product.Stock -= itemRequest.Quantity;
                _productRepository.Update(product);

                totalAmount += orderItem.Quantity * orderItem.UnitPrice;
            }

            // Atualizar total do pedido
            order.TotalAmount = totalAmount;
            _orderRepository.Update(order);

            // ✅ Salvar tudo em uma única transação via Unit of Work
            // Se qualquer operação falhar, toda a transação é revertida
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Pedido criado com sucesso: {OrderId} - Total: {TotalAmount:C}",
                order.Id, order.TotalAmount);

            return new OrderDto
            {
                Id = order.Id,
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ItemCount = itemRequests.Count()
            };
        }
        catch (Exception ex)
        {
            // ✅ Em caso de erro, a transação é automaticamente revertida
            _logger.LogError(ex, "Erro ao criar pedido para cliente {CustomerId}", customerId);
            throw;
        }
    }

    #endregion

    #region 4. Queries de Agregação

    /// <summary>
    /// Exemplo 8: Agregações e estatísticas via Repository
    /// Demonstra uso de GroupBy, Sum, Average etc.
    /// </summary>
    public async Task<SalesReportDto> GetSalesReportAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            // ✅ Agregação de vendas por período usando Repository
            var orders = await _orderRepository.GetAll()
                .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate && o.IsActive)
                .Include(o => o.Items)
                .ToListAsync();

            // ✅ Calcular estatísticas
            var report = new SalesReportDto
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalOrders = orders.Count,
                TotalRevenue = orders.Sum(o => o.TotalAmount),
                AverageOrderValue = orders.Any() ? orders.Average(o => o.TotalAmount) : 0,

                // Vendas por status
                OrdersByStatus = orders
                    .GroupBy(o => o.Status)
                    .ToDictionary(g => g.Key.ToString(), g => g.Count()),

                // Top produtos
                TopProducts = orders
                    .SelectMany(o => o.Items)
                    .GroupBy(i => new { i.ProductId, i.Product!.Name })
                    .Select(g => new ProductSalesDto
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        Quantity = g.Sum(i => i.Quantity),
                        Revenue = g.Sum(i => i.Quantity * i.UnitPrice)
                    })
                    .OrderByDescending(p => p.Revenue)
                    .Take(10)
                    .ToList()
            };

            _logger.LogInformation("Relatório gerado: {TotalOrders} pedidos, {TotalRevenue:C} receita",
                report.TotalOrders, report.TotalRevenue);

            return report;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar relatório de vendas");
            throw;
        }
    }

    #endregion

    #region 5. Operações em Lote (Batch)

    /// <summary>
    /// Exemplo 9: Operações em lote para melhor performance via Repository
    /// Demonstra como processar múltiplos registros eficientemente
    /// </summary>
    public async Task<int> UpdateProductPricesAsync(Dictionary<int, decimal> priceUpdates)
    {
        try
        {
            var productIds = priceUpdates.Keys.ToList();

            // ✅ Buscar todos os produtos em uma única query via Repository
            var products = await _productRepository.GetAll()
                .Where(p => productIds.Contains(p.Id) && p.IsActive)
                .ToListAsync();

            int updatedCount = 0;

            // ✅ Atualizar preços em lote via Repository
            foreach (var product in products)
            {
                if (priceUpdates.TryGetValue(product.Id, out var newPrice) && newPrice != product.Price)
                {
                    product.Price = newPrice;
                    product.LastUpdate = DateTime.UtcNow;
                    _productRepository.Update(product);
                    updatedCount++;
                }
            }

            // ✅ Salvar todas as mudanças em uma única transação via Unit of Work
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Preços atualizados: {UpdatedCount} de {TotalCount} produtos",
                updatedCount, products.Count);

            return updatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar preços em lote");
            throw;
        }
    }

    #endregion
}

#region DTOs (Data Transfer Objects)

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class ProductSummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public bool InStock { get; set; }
    public int DaysOld { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class SalesReportDto
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public Dictionary<string, int> OrdersByStatus { get; set; } = new();
    public List<ProductSalesDto> TopProducts { get; set; } = new();
}

public class ProductSalesDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

#endregion

#region Request Models

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

#endregion
