using Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Repositories;

[Collection(nameof(DataCollection))]
[Trait("Category", "Integration")]
public class AutoHistoryTest
{
    private readonly AppDbContextFixture _dbContext;
    private readonly DataFixture _dataFixture;
    private readonly SeedDbFixture _seedDbFixture;

    private readonly Repository<Fatura> _faturaRepository;
    private const string UserRequest = "UoWSqlServerUserTest";

    public AutoHistoryTest(
        AppDbContextFixture dbContext,
        DataFixture dataFixture,
        SeedDbFixture seedDbFixture)
    {
        _dbContext = dbContext;
        _dataFixture = dataFixture;
        _seedDbFixture = seedDbFixture;

        using var uow = new UnitOfWork<DbContext>(_dbContext.Db)
        {
            UsernameContext = UserRequest
        };

        _faturaRepository = new Repository<Fatura>(_dbContext.Db, uow);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "AutoHistory")]
    public async Task DomainEvent_ComAutoHistory_ComClassesIguaisComVersoesDiferentes_DeveGerarEventComAmbasVersoes()
    {
        // Arrange
        const int RegistrosGravadosMaisHistory = 2;

        _seedDbFixture.CreateData(
            _dbContext,
            _dataFixture,
            UserRequest,
            2, 5, true);

        // Act
        var fatura = await _faturaRepository.GetFirstOrDefaultAsync(predicate:
            x => x.Id == _seedDbFixture.Fatura.Id);

        var faturaNewVersion = await _faturaRepository.FindAsync(fatura.Id);
        faturaNewVersion.Update("Essa é a versão 2 da fatura");

        var registries = await _faturaRepository.SaveChangesAsync(ensureAutoHistory: true);

        var faturaUpdated = new FaturaUpdatedEvent(fatura, "XYZ", faturaNewVersion);

        // Assert
        Assert.Equal(RegistrosGravadosMaisHistory, registries);
        Assert.True(faturaUpdated.SourceId.Equals(faturaUpdated.NewFatura));
        Assert.False(faturaUpdated.SourceId.Observacao == faturaUpdated.NewFatura.Observacao);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "AutoHistory")]
    public async Task AutoHistory_ComCorrelationId_DeveGravarCampoCorrelationId()
    {
        // Arrange
        _dbContext.PreventDisposal = false;
        const int RegistrosGravadosMaisHistory = 2;

        // Ensure we have data for this test
        if (_seedDbFixture.Fatura == null)
        {
            _seedDbFixture.CreateData(
                _dbContext,
                _dataFixture,
                UserRequest,
                1, 1, true);
        }

        // Act
        var fatura = await _faturaRepository.FindAsync(_seedDbFixture.Fatura.Id);
        fatura.Update("Testando correlationId no AutoHistory");

        var registries = await _faturaRepository.SaveChangesAsync(ensureAutoHistory: true);

        // Assert
        Assert.Equal(RegistrosGravadosMaisHistory, registries);
    }

}
