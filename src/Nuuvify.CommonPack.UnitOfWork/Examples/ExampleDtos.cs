namespace Nuuvify.CommonPack.UnitOfWork.Examples;

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
    public ICollection<OrderItemDto> Items { get; set; } = new List<OrderItemDto>();
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
    public ICollection<ProductSalesDto> TopProducts { get; set; } = new List<ProductSalesDto>();
}

public class ProductSalesDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class ProductStatsDto
{
    public int TotalProducts { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int TotalStock { get; set; }
    public ICollection<CategoryStatsDto> CategoryStats { get; set; } = new List<CategoryStatsDto>();
}

public class CategoryStatsDto
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal AveragePrice { get; set; }
    public int TotalStock { get; set; }
}

#endregion

#region Request Models

public class OrderItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

#endregion
