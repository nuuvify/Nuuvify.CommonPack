using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.UnitOfWork.Examples.Shared;

/// <summary>
/// Modelo de entidade Product compartilhado entre todos os exemplos.
/// Centraliza a definição para evitar ambiguidades.
/// </summary>
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
}

/// <summary>
/// Modelo de entidade Order compartilhado entre todos os exemplos.
/// </summary>
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public bool IsActive { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

/// <summary>
/// Modelo de entidade OrderItem compartilhado entre todos os exemplos.
/// </summary>
public class OrderItem
{
    public int Id { get; set; }
    public Order Order { get; set; } = null!;
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

/// <summary>
/// Modelo de entidade Customer compartilhado entre todos os exemplos.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Enum para status do pedido.
/// </summary>
public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Delivered,
    Cancelled
}

/// <summary>
/// DbContext de exemplo compartilhado entre todos os exemplos.
/// </summary>
public class ExampleDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Customer> Customers { get; set; }

    public ExampleDbContext(DbContextOptions<ExampleDbContext> options) : base(options) { }
}