using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Examples;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fixtures;

/// <summary>
/// Fixture para testes com banco de dados EF Core In-Memory.
/// Cada classe de teste recebe uma instância única do banco com nome GUID.
///
/// IMPORTANTE: O banco é compartilhado entre TODOS os testes da mesma classe.
/// Novos contextos são criados para cada operação, mas compartilham o mesmo banco In-Memory.
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

        // Cria um contexto inicial apenas para garantir que o banco seja criado
        using var initialContext = new ExampleDbContext(_options);
        _ = initialContext.Database.EnsureCreated();
    }

    public ExampleDbContext CreateContext()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ProductDbContextFixture));

        // Cria um novo contexto para cada chamada, mas usando o mesmo banco In-Memory
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
