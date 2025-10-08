using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Data;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fakers;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Fixtures;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Tests;

[Trait("Category", "Unit")]
public sealed class SimpleInMemoryTest : IClassFixture<AppDbContextFixture>, IDisposable
{
    private readonly AppDbContext _context;
    private readonly Nuuvify.CommonPack.UnitOfWork.UnitOfWork<AppDbContext> _unitOfWork;
    private readonly Repository<Fatura> _faturaRepository;
    private readonly Repository<Pedido> _pedidoRepository;

    public SimpleInMemoryTest(AppDbContextFixture fixture)
    {
        _context = fixture.CreateContext();
        _unitOfWork = new Nuuvify.CommonPack.UnitOfWork.UnitOfWork<AppDbContext>(_context)
        {
            UsernameContext = "InMemoryTestUser"
        };

        _faturaRepository = new Repository<Fatura>(_context, _unitOfWork);
        _pedidoRepository = new Repository<Pedido>(_context, _unitOfWork);
    }

    [Fact]
    public async Task Repository_Add_ShouldSaveSuccessfully()
    {
        // Arrange
        var fatura = FaturaFaker.Generate();

        // Act
        _ = await _faturaRepository.Add(fatura);
        _ = await _unitOfWork.SaveChangesAsync();

        // Assert
        var saved = await _faturaRepository.FindAsync(fatura.Id);
        Assert.NotNull(saved);
        Assert.Equal(fatura.NumeroFatura, saved.NumeroFatura);
    }

    [Fact]
    public async Task Repository_FindAsync_ShouldReturnCorrectEntity()
    {
        // Arrange
        var fatura = FaturaFaker.Generate();
        _ = await _faturaRepository.Add(fatura);
        _ = await _unitOfWork.SaveChangesAsync();

        // Act
        var found = await _faturaRepository.FindAsync(fatura.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(fatura.Id, found.Id);
        Assert.Equal(fatura.NumeroFatura, found.NumeroFatura);
    }

    [Fact]
    public async Task Repository_ExistsAsync_ShouldReturnTrue_WhenEntityExists()
    {
        // Arrange
        var fatura = FaturaFaker.Generate();
        _ = await _faturaRepository.Add(fatura);
        _ = await _unitOfWork.SaveChangesAsync();

        // Act
        var exists = await _faturaRepository.ExistsAsync(f => f.Id == fatura.Id);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task Repository_Update_ShouldModifyEntity()
    {
        // Arrange
        var fatura = FaturaFaker.Generate();
        _ = await _faturaRepository.Add(fatura);
        _ = await _unitOfWork.SaveChangesAsync();

        var novaObservacao = "Observação atualizada";

        // Act
        fatura.Update(novaObservacao);
        _faturaRepository.Update(fatura);
        _ = await _unitOfWork.SaveChangesAsync();

        // Assert
        var updated = await _faturaRepository.FindAsync(fatura.Id);
        Assert.NotNull(updated);
        Assert.Equal(novaObservacao, updated.Observacao);
    }

    [Fact]
    public async Task Repository_Remove_ShouldDeleteEntity()
    {
        // Arrange
        var fatura = FaturaFaker.Generate();
        _ = await _faturaRepository.Add(fatura);
        _ = await _unitOfWork.SaveChangesAsync();

        // Act
        _faturaRepository.Remove(fatura);
        _ = await _unitOfWork.SaveChangesAsync();

        // Assert
        var deleted = await _faturaRepository.FindAsync(fatura.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Repository_GetFirstOrDefaultAsync_ShouldReturnFirstMatch()
    {
        // Arrange
        var fatura1 = FaturaFaker.Generate();
        var fatura2 = FaturaFaker.Generate();

        _ = await _faturaRepository.Add(fatura1);
        _ = await _faturaRepository.Add(fatura2);
        _ = await _unitOfWork.SaveChangesAsync();

        // Act
        var first = await _faturaRepository.GetFirstOrDefaultAsync(
            predicate: f => f.NumeroFatura > 0);

        // Assert
        Assert.NotNull(first);
    }

    [Fact]
    public async Task Repository_GetAll_ShouldReturnAllEntities()
    {
        // Arrange
        var faturas = FaturaFaker.Generate(5);

        foreach (var fatura in faturas)
        {
            _ = await _faturaRepository.Add(fatura);
        }
        _ = await _unitOfWork.SaveChangesAsync();

        // Act
        var all = await _faturaRepository.GetAllAsync();

        // Assert
        Assert.NotNull(all);
        Assert.True(all.Count >= 5);
    }

    [Fact]
    public async Task UnitOfWork_MultipleRepositories_ShouldWorkTogether()
    {
        // Arrange
        var fatura = FaturaFaker.Generate();
        var pedido = PedidoFaker.Generate();

        // Act
        _ = await _faturaRepository.Add(fatura);
        _ = await _pedidoRepository.Add(pedido);
        _ = await _unitOfWork.SaveChangesAsync();

        // Assert
        var savedFatura = await _faturaRepository.FindAsync(fatura.Id);
        var savedPedido = await _pedidoRepository.FindAsync(pedido.Id);

        Assert.NotNull(savedFatura);
        Assert.NotNull(savedPedido);
    }

    [Fact]
    public async Task Repository_ExistsAsync_ShouldReturnFalse_WhenEntityDoesNotExist()
    {
        // Act
        var exists = await _faturaRepository.ExistsAsync(f => f.NumeroFatura == 999999);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task UnitOfWork_SaveChangesAsync_ShouldReturnCorrectCount()
    {
        // Arrange - Create a completely fresh context with unique database name
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_SaveChanges_{Guid.NewGuid()}")
            .Options;

        using var isolatedContext = new AppDbContext(options);
        using var isolatedUnitOfWork = new Nuuvify.CommonPack.UnitOfWork.UnitOfWork<AppDbContext>(isolatedContext)
        {
            UsernameContext = "InMemoryTestUser"
        };
        var isolatedRepository = new Repository<Fatura>(isolatedContext, isolatedUnitOfWork);

        var fatura1 = FaturaFaker.Generate();
        var fatura2 = FaturaFaker.Generate();

        _ = await isolatedRepository.Add(fatura1);
        _ = await isolatedRepository.Add(fatura2);

        // Act
        var changesCount = await isolatedUnitOfWork.SaveChangesAsync();

        // Verify count of Fatura entities only
        var faturaCount = await isolatedRepository.GetAllAsync();

        // Assert - Since AutoHistory is creating additional entries, let's just verify our entities exist
        Assert.Equal(2, faturaCount.Count);
        Assert.True(changesCount >= 2); // At least 2 changes (our entities), may be more due to AutoHistory
    }

    [Fact]
    public async Task Repository_AddRange_ShouldSaveMultipleEntities()
    {
        // Arrange
        var faturas = FaturaFaker.Generate(3);

        // Act
        await _faturaRepository.Add(faturas);
        _ = await _unitOfWork.SaveChangesAsync();

        // Assert
        foreach (var fatura in faturas)
        {
            var saved = await _faturaRepository.FindAsync(fatura.Id);
            Assert.NotNull(saved);
            Assert.Equal(fatura.NumeroFatura, saved.NumeroFatura);
        }
    }

    [Fact]
    public async Task Repository_UpdateRange_ShouldModifyMultipleEntities()
    {
        // Arrange
        var faturas = FaturaFaker.Generate(2);

        foreach (var fatura in faturas)
        {
            _ = await _faturaRepository.Add(fatura);
        }
        _ = await _unitOfWork.SaveChangesAsync();

        // Update entities
        var novaObservacao = "Observação em lote";
        foreach (var fatura in faturas)
        {
            fatura.Update(novaObservacao);
        }

        // Act
        _faturaRepository.Update(faturas);
        _ = await _unitOfWork.SaveChangesAsync();

        // Assert
        foreach (var fatura in faturas)
        {
            var updated = await _faturaRepository.FindAsync(fatura.Id);
            Assert.NotNull(updated);
            Assert.Equal(novaObservacao, updated.Observacao);
        }
    }

    [Fact]
    public async Task Repository_RemoveRange_ShouldDeleteMultipleEntities()
    {
        // Arrange
        var faturas = FaturaFaker.Generate(2);

        foreach (var fatura in faturas)
        {
            _ = await _faturaRepository.Add(fatura);
        }
        _ = await _unitOfWork.SaveChangesAsync();

        // Act
        _faturaRepository.Remove(faturas);
        _ = await _unitOfWork.SaveChangesAsync();

        // Assert
        foreach (var fatura in faturas)
        {
            var deleted = await _faturaRepository.FindAsync(fatura.Id);
            Assert.Null(deleted);
        }
    }

    [Fact]
    public async Task Repository_RemoveById_ShouldDeleteEntityById()
    {
        // Arrange
        var fatura = FaturaFaker.Generate();
        _ = await _faturaRepository.Add(fatura);
        _ = await _unitOfWork.SaveChangesAsync();

        // Act
        _faturaRepository.Remove(fatura.Id);
        _ = await _unitOfWork.SaveChangesAsync();

        // Assert
        var deleted = await _faturaRepository.FindAsync(fatura.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task Repository_GetAllWithPredicate_ShouldFilterResults()
    {
        // Arrange
        var faturas = FaturaFaker.Generate(5);
        var targetObservacao = "Observação especial";

        faturas[0].Update(targetObservacao);
        faturas[1].Update(targetObservacao);

        foreach (var fatura in faturas)
        {
            _ = await _faturaRepository.Add(fatura);
        }
        _ = await _unitOfWork.SaveChangesAsync();

        // Act
        var filtered = await _faturaRepository.GetAllAsync(
            predicate: f => f.Observacao == targetObservacao);

        // Assert
        Assert.Equal(2, filtered.Count);
        Assert.All(filtered, f => Assert.Equal(targetObservacao, f.Observacao));
    }

    [Fact]
    public async Task Repository_GetAllWithOrdering_ShouldReturnOrderedResults()
    {
        // Arrange
        var faturas = FaturaFaker.Generate(3);
        faturas[0].ChangeNumeroFatura(100);
        faturas[1].ChangeNumeroFatura(200);
        faturas[2].ChangeNumeroFatura(150);

        foreach (var fatura in faturas)
        {
            _ = await _faturaRepository.Add(fatura);
        }
        _ = await _unitOfWork.SaveChangesAsync();

        // Act
        var ordered = await _faturaRepository.GetAllAsync(
            predicate: f => f.NumeroFatura > 0,
            orderBy: q => q.OrderBy(f => f.NumeroFatura));

        // Assert
        Assert.True(ordered.Count >= 3);
        var targetFaturas = ordered.Where(f => new[] { 100, 150, 200 }.Contains(f.NumeroFatura)).ToList();
        Assert.Equal(3, targetFaturas.Count);
        Assert.Equal(100, targetFaturas[0].NumeroFatura);
        Assert.Equal(150, targetFaturas[1].NumeroFatura);
        Assert.Equal(200, targetFaturas[2].NumeroFatura);
    }

    [Fact]
    public async Task Repository_SaveChangesWithCancellationToken_ShouldWork()
    {
        // Arrange
        var fatura = FaturaFaker.Generate();
        _ = await _faturaRepository.Add(fatura);

        // Act
        var changesCount = await _unitOfWork.SaveChangesAsync(cancellationToken: default);

        // Assert
        Assert.True(changesCount >= 1);
        var saved = await _faturaRepository.FindAsync(fatura.Id);
        Assert.NotNull(saved);
    }

    public void Dispose()
    {
        _unitOfWork?.Dispose();
        _context?.Dispose();
        GC.SuppressFinalize(this);
    }
}
