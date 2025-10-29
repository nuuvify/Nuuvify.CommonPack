using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Examples;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fixtures;

/// <summary>
/// Fixture para testes com banco de dados EF Core In-Memory.
/// Cada classe de teste recebe uma instância única do banco com nome GUID.
///
/// IMPORTANTE: O banco é compartilhado entre TODOS os testes da mesma classe.
/// Um único contexto é mantido e reutilizado para garantir persistência dos dados.
/// </summary>
public sealed class ProductDbContextFixture : IDisposable
{
    private readonly string _databaseName;
    private readonly DbContextOptions<ExampleDbContext> _options;
    private readonly ExampleDbContext _sharedContext;
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

        // Cria um contexto compartilhado para todos os testes
        _sharedContext = new ExampleDbContext(_options);
        _ = _sharedContext.Database.EnsureCreated();
    }

    public ExampleDbContext CreateContext()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(ProductDbContextFixture));

        // Retorna o contexto compartilhado para manter os dados entre testes
        return _sharedContext;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _sharedContext?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
