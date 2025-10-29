using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Examples;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fixtures;

/// <summary>
/// Fixture para testes com banco de dados EF Core In-Memory.
/// Cada classe de teste recebe uma instância única do banco com nome GUID.
/// 
/// IMPORTANTE: O banco é compartilhado entre TODOS os testes da mesma classe.
/// Para garantir isolamento, cada teste deve limpar seus dados no Dispose().
/// </summary>
public sealed class ProductDbContextFixture : IDisposable
{
    private readonly string _databaseName;
    private readonly DbContextOptions<ExampleDbContext> _options;
    private bool _disposed = false;

    public ProductDbContextFixture()
    {
        // Cria um banco In-Memory único para cada execução da classe de teste
        _databaseName = $"ProductTestDb_{Guid.NewGuid()}";

        _options = new DbContextOptionsBuilder<ExampleDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        // Inicializa o schema do banco de dados
        using var context = new ExampleDbContext(_options);
        _ = context.Database.EnsureCreated();
    }

    public ExampleDbContext CreateContext()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ProductDbContextFixture));

        return new ExampleDbContext(_options);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
