using System;
using System.Linq;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Repositories;

[Collection(nameof(DataCollection))]
[Trait("Category", "Integration")]
public class FaturaRepository
{
    private readonly AppDbContextFixture _dbContext;
    private readonly DataFixture _dataFixture;
    private readonly SeedDbFixture _seedDbFixture;

    private readonly Repository<Fatura> _faturaRepository;
    private const string UserRequest = "SqlServerUserTest";

    public FaturaRepository(
        AppDbContextFixture dbContext,
        DataFixture dataFixture,
        ITestOutputHelper outputHelper,
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
    [Trait("SqlServer", "Fatura Repository - Write")]
    public async Task DomainEvent_ComClassesIguaisComVersoesDiferentes_DeveGerarEventComAmbasVersoes()
    {
        // Arrange - Criar dados específicos para este teste
        _seedDbFixture.CreateData(
            _dbContext,
            _dataFixture,
            UserRequest,
            2, 5, true);

        const int RegistriesSaved = 1;

        // Act
        var fatura = await _faturaRepository.GetFirstOrDefaultAsync(predicate:
            x => x.Id == _seedDbFixture.Fatura.Id);

        var faturaNewVersion = await _faturaRepository.FindAsync(fatura.Id);
        faturaNewVersion.Update("Essa é a versão 2 da fatura");

        var registries = await _faturaRepository.SaveChangesAsync();

        // Assert
        var faturaUpdated = new FaturaUpdatedEvent(fatura, "XYZ", faturaNewVersion);

        Assert.Equal(RegistriesSaved, registries);
        Assert.True(faturaUpdated.SourceId.Equals(faturaUpdated.NewFatura));
        Assert.False(faturaUpdated.SourceId.Observacao == faturaUpdated.NewFatura.Observacao);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task FindAsync_DeveRetornarEntidadeNotNull_MesmoSemEncontrarId()
    {
        // Arrange & Act
        var faturaFinded = await _faturaRepository.FindAsync(keyValues: Guid.NewGuid().ToString());

        // Assert
        Assert.Null(faturaFinded);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - Write")]
    public void FromSqlDeveRetornarEntidadeValida()
    {
        // Arrange - Criar dados específicos para este teste
        _seedDbFixture.CreateData(
            _dbContext,
            _dataFixture,
            UserRequest,
            2, 5, true);

        var sql = $"SELECT * FROM {_dbContext.Schema}.FATURAS WHERE NUMERO_FATURA = {_seedDbFixture.Fatura.NumeroFatura}";

        // Act
        var faturaFinded = _faturaRepository.FromSql(sql).ToList();

        // Assert
        Assert.True(faturaFinded.Count > 0);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task FirstOrDefaultComPredicateDeveRetornarEntidadeValida()
    {
        // Arrange - Criar dados específicos para este teste
        _seedDbFixture.CreateData(
            _dbContext,
            _dataFixture,
            UserRequest,
            2, 5, true);

        // Act
        var faturaFinded = await _faturaRepository.GetFirstOrDefaultAsync(predicate: x =>
            x.NumeroFatura == _seedDbFixture.Fatura.NumeroFatura,
            disableTracking: false);

        // Assert
        Assert.NotNull(faturaFinded);
        Assert.Equal(_seedDbFixture.Fatura.NumeroFatura, faturaFinded.NumeroFatura);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task FirstOrDefaultComPredicateComIncludeDeveRetornarEntidadeValida()
    {
        // Arrange
        const int npedido = 3;

        var fatura = _dataFixture.GerarFaturaFake().First();
        var pedidos = _dataFixture.GerarPedidoFake(npedido, 6);

        fatura.AdicionarPedido(pedidos);

        var faturaAdded = await _faturaRepository.Add(fatura);
        var saveChangesResult = await _faturaRepository.SaveChangesAsync();

        var numeroPedido = fatura.Pedidos.LastOrDefault().NumeroPedido;

        var faturaFinded = await _faturaRepository.GetFirstOrDefaultAsync(
            predicate: x => x.NumeroFatura == fatura.NumeroFatura,
            disableTracking: false);

        var faturaPedido = faturaFinded.Pedidos.FirstOrDefault(x => x.NumeroPedido == numeroPedido);

        Assert.NotNull(faturaFinded);
        Assert.Equal(numeroPedido, faturaPedido.NumeroPedido);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task GetListComPredicateDeveRetornarEntidadeValida()
    {
        // Arrange - Criar dados específicos para este teste
        _seedDbFixture.CreateData(
            _dbContext,
            _dataFixture,
            UserRequest,
            2, 5, true);

        var hoje = new DateTime(DateTime.Today.Year,
            DateTime.Today.Month,
            DateTime.Today.Day);

        // Act
        var faturasFinded = await _faturaRepository.GetPagedListAsync(
            predicate: x => x.NumeroFatura == _seedDbFixture.Fatura.NumeroFatura &&
            x.DataCadastro >= hoje,
            disableTracking: false,
            ignoreQueryFilters: true);

        // Assert
        Assert.NotNull(faturasFinded);
        Assert.True(faturasFinded.Items.NotNullOrZero());
    }

}
