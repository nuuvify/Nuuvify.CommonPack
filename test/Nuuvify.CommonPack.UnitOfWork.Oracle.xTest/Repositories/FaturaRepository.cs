using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Fixtures;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Repositories;

[Collection(nameof(DataCollection))]
public class FaturaRepository
{
    private readonly AppDbContextFixture _dbContext;
    private readonly DataFixture _dataFixture;
    private readonly SeedDbFixture _seedDbFixture;
    private readonly ITestOutputHelper _outputHelper;

    private readonly Repository<Fatura> _faturaRepository;
    private const string UserRequest = "UoWOracleUserTest";

    public FaturaRepository(
        AppDbContextFixture dbContext,
        DataFixture dataFixture,
        ITestOutputHelper outputHelper,
        SeedDbFixture seedDbFixture)
    {
        _dbContext = dbContext;
        _dataFixture = dataFixture;
        _outputHelper = outputHelper;
        _seedDbFixture = seedDbFixture;

        var _uow = new UnitOfWork<DbContext>(_dbContext.Db)
        {
            UsernameContext = UserRequest
        };

        _faturaRepository = new Repository<Fatura>(_dbContext.Db, _uow);

    }

    [OracleTestFact, Order(1)]
    [Trait("Oracle", "Fatura Repository - Write")]
    public async Task DomainEvent_ComClassesIguaisComVersoesDiferentes_DeveGerarEventComAmbasVersoes()
    {
        _seedDbFixture.CreateData(_outputHelper,
            _dbContext,
            _dataFixture,
            UserRequest,
            2, 5, true);

        const int RegistriesSaved = 1;

        var fatura = await _faturaRepository.GetFirstOrDefaultAsync(predicate:
            x => x.Id == _seedDbFixture.Fatura.Id);

        var faturaNewVersion = await _faturaRepository.FindAsync(fatura.Id);
        faturaNewVersion.Update("Essa é a versão 2 da fatura");

        var registries = await _faturaRepository.SaveChangesAsync();

        var faturaUpdated = new FaturaUpdatedEvent(fatura, "XYZ", faturaNewVersion);

        Assert.Equal(RegistriesSaved, registries);
        Assert.True(faturaUpdated.SourceId.Equals(faturaUpdated.NewFatura));
        Assert.False(faturaUpdated.SourceId.Observacao == faturaUpdated.NewFatura.Observacao);
    }

    [OracleTestFact, Order(2)]
    [Trait("Oracle", "Fatura Repository - Write")]
    public void FromSqlDeveRetornarEntidadeValida()
    {

        var sql = $"SELECT * FROM {_dbContext.Schema}.FATURAS WHERE NUMERO_FATURA = {_seedDbFixture.Fatura.NumeroFatura}";

        var faturaFinded = _faturaRepository.FromSql(sql).ToList();

        Assert.True(faturaFinded.Count > 0);
    }

    [OracleTestFact, Order(3)]
    [Trait("Oracle", "Fatura Repository - Write")]
    public async Task FirstOrDefaultComPredicateComIncludeDeveRetornarEntidadeValida()
    {

        const int npedido = 3;

        var fatura = _dataFixture.GerarFaturaFake().First();
        var pedidos = _dataFixture.GerarPedidoFake(npedido, 6);

        fatura.AdicionarPedido(pedidos);

        _ = await _faturaRepository.Add(fatura);
        var registries = await _faturaRepository.SaveChangesAsync();

        Assert.Equal(22, registries);
    }

    [OracleTestFact, Order(4)]
    [UseCulture("en-US", "en-US")]
    [Trait("Oracle", "Fatura Repository - Write")]
    public async Task RemoveFaturaPorIdNaoDeveRemoverCasoExistaPedido()
    {
        _dbContext.PreventDisposal = false;

        const string messageExpected = "(\"B8CT2\".\"PEDIDOS\".\"FATURA_ID\")";

        _faturaRepository.Remove(_seedDbFixture.Fatura.Id);

        var resultException = await Assert.ThrowsAsync<CustomException>(()
            => _faturaRepository.SaveChangesAsync());

        Assert.True(resultException.CustomErrors.Count > 0);
        Assert.Equal(UserRequest, _dbContext.Db.GetDbContextUsername());
        Assert.Contains(messageExpected, resultException.Message);

    }

}
