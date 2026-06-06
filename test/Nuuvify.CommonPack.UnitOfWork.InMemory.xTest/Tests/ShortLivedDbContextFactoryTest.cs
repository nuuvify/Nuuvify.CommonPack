using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Data;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
public sealed class ShortLivedDbContextFactoryTest
{
    [Fact]
    public async Task CreateAsync_ShouldCreateIndependentDbContexts()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ShortLivedFactoryDb_{Guid.NewGuid()}")
            .Options;
        var dbContextFactory = new TrackingDbContextFactory(options);
        IShortLivedDbContextFactory<AppDbContext> factory =
            new ShortLivedDbContextFactory<AppDbContext>(dbContextFactory);

        // Act
        var first = await factory.CreateAsync("worker-a", "user-a");
        var second = await factory.CreateAsync("worker-b", "user-b");

        // Assert
        Assert.NotSame(first, second);
        Assert.Equal(2, dbContextFactory.CreatedCount);

        first.Dispose();
        second.Dispose();
    }

    [Fact]
    public async Task CreateAsync_ShouldApplyAuditContext()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"ShortLivedFactoryAuditDb_{Guid.NewGuid()}")
            .Options;
        var dbContextFactory = new TrackingDbContextFactory(options);
        IShortLivedDbContextFactory<AppDbContext> factory =
            new ShortLivedDbContextFactory<AppDbContext>(dbContextFactory);

        // Act
        var dbContext = await factory.CreateAsync("worker-audit", "user-audit");

        // Assert
        Assert.Equal("worker-audit", dbContext.GetDbContextUsername());
        Assert.Equal("user-audit", dbContext.GetDbContextUserId());

        dbContext.Dispose();
    }

    [Fact]
    public void AddShortLivedDbContextFactory_ShouldRegisterOptInFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddDbContextFactory<AppDbContext>(options =>
            options.UseInMemoryDatabase($"ShortLivedFactoryRegistrationDb_{Guid.NewGuid()}"));

        // Act
        _ = services.AddShortLivedDbContextFactory<AppDbContext>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var factory = scope.ServiceProvider.GetService<IShortLivedDbContextFactory<AppDbContext>>();

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public async Task WorkerDbContextFactory_ShouldCreateDbContextWithAuditContext()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"WorkerFactoryAuditDb_{Guid.NewGuid()}")
            .Options;
        var dbContextFactory = new TrackingDbContextFactory(options);
        IWorkerDbContextFactory<AppDbContext> factory =
            new WorkerDbContextFactory<AppDbContext>(dbContextFactory);

        // Act
        var dbContext = await factory.CreateAsync("worker-audit", "worker-id");

        // Assert
        Assert.Equal("worker-audit", dbContext.GetDbContextUsername());
        Assert.Equal("worker-id", dbContext.GetDbContextUserId());

        dbContext.Dispose();
    }

    [Fact]
    public void AddWorkerDbContextFactory_ShouldRegisterOptInFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddDbContextFactory<AppDbContext>(options =>
            options.UseInMemoryDatabase($"WorkerFactoryRegistrationDb_{Guid.NewGuid()}"));

        // Act
        _ = services.AddWorkerDbContextFactory<AppDbContext>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var factory = scope.ServiceProvider.GetService<IWorkerDbContextFactory<AppDbContext>>();

        // Assert
        Assert.NotNull(factory);
    }

    private sealed class TrackingDbContextFactory : IDbContextFactory<AppDbContext>
    {
        private readonly DbContextOptions<AppDbContext> _options;
        private readonly List<AppDbContext> _created = [];

        public TrackingDbContextFactory(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public int CreatedCount => _created.Count;

        public AppDbContext CreateDbContext()
        {
            var context = new AppDbContext(_options);
            _created.Add(context);
            return context;
        }

        public Task<AppDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(CreateDbContext());
        }
    }
}
