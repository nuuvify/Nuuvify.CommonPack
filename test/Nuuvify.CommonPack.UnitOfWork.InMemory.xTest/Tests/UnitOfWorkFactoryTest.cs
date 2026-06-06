using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Interfaces;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Data;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
[Collection("DbContextExtensionsStaticState")]
public sealed class UnitOfWorkFactoryTest
{
    [Fact]
    public async Task CreateAsync_ShouldCreateIndependentUnitsOfWork()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"FactoryDb_{Guid.NewGuid()}")
            .Options;
        var dbContextFactory = new TrackingDbContextFactory(options);
        IUnitOfWorkFactory<AppDbContext> factory = new UnitOfWorkFactory<AppDbContext>(dbContextFactory);

        // Act
        var first = await factory.CreateAsync("worker-a", "user-a");
        var second = await factory.CreateAsync("worker-b", "user-b");

        // Assert
        Assert.NotSame(first, second);
        Assert.NotSame(first.DbContext, second.DbContext);
        Assert.Equal(2, dbContextFactory.CreatedCount);
        Assert.Equal("worker-a", first.UsernameContext);
        Assert.Equal("user-a", first.UserIdContext);
        Assert.Equal("worker-b", second.UsernameContext);
        Assert.Equal("user-b", second.UserIdContext);

        first.Dispose();
        second.Dispose();
    }

    [Fact]
    public async Task Dispose_ShouldDisposeCreatedDbContext()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"FactoryDisposeDb_{Guid.NewGuid()}")
            .Options;
        var dbContextFactory = new TrackingDbContextFactory(options);
        IUnitOfWorkFactory<AppDbContext> factory = new UnitOfWorkFactory<AppDbContext>(dbContextFactory);
        var unitOfWork = await factory.CreateAsync("worker-dispose", "user-dispose");

        _ = await unitOfWork.DbContext.Set<Fatura>().CountAsync();

        // Act
        unitOfWork.Dispose();

        // Assert
        await Assert.ThrowsAsync<ObjectDisposedException>(async () =>
        {
            _ = await unitOfWork.DbContext.Set<Fatura>().CountAsync();
        });
    }

    [Fact]
    public void AddUnitOfWorkFactory_ShouldRegisterOptInFactory()
    {
        // Arrange
        var services = new ServiceCollection();
        _ = services.AddDbContextFactory<AppDbContext>(options =>
            options.UseInMemoryDatabase($"FactoryRegistrationDb_{Guid.NewGuid()}"));

        // Act
        _ = services.AddUnitOfWorkFactory<AppDbContext>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var factory = scope.ServiceProvider.GetService<IUnitOfWorkFactory<AppDbContext>>();

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
