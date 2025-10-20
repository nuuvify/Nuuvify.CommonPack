using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Data;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fixtures;

public sealed class AppDbContextFixture : IDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;
    private bool _disposed = false;

    public AppDbContextFixture()
    {
        // Configure In Memory Database with unique name for each test run
        var databaseName = $"InMemoryTestDb_{Guid.NewGuid()}";

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        // Initialize the database schema
        using var context = new AppDbContext(_options);
        _ = context.Database.EnsureCreated();
    }

    public AppDbContext CreateContext()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(AppDbContextFixture));

        return new AppDbContext(_options);
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

[CollectionDefinition("AppDbContext Collection")]
public class AppDbContextCollection : ICollectionFixture<AppDbContextFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
