using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Repositories;

[Collection(nameof(DataCollection))]
[Trait("Category", "Integration")]
public class FaturaRepositoryReadOnly
{
    private readonly AppDbContextFixture _dbContext;
    private readonly DataFixture _dataFixture;
    private readonly SeedDbFixture _seedDbFixture;
    private readonly ITestOutputHelper _outputHelper;

    private readonly Repository<Fatura> _faturaRepository;
    private const string UserRequest = "SqlServerUserTest";

    public FaturaRepositoryReadOnly(
        AppDbContextFixture dbContext,
        DataFixture dataFixture,
        ITestOutputHelper outputHelper,
        SeedDbFixture seedDbFixture)
    {
        _dbContext = dbContext;
        _dataFixture = dataFixture;
        _outputHelper = outputHelper;
        _seedDbFixture = seedDbFixture;

        var uow = new UnitOfWork<DbContext>(_dbContext.Db)
        {
            UsernameContext = UserRequest
        };

        _faturaRepository = new Repository<Fatura>(_dbContext.Db, uow);

    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task FindAsyncDeveRetornarEntidadeValida()
    {

        _seedDbFixture.CreateData(
            _dbContext,
            _dataFixture,
            UserRequest,
            2, 5, true);

        var faturaFinded = await _faturaRepository.FindAsync(keyValues: _seedDbFixture.Fatura.Id);

        Assert.NotNull(faturaFinded);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task FindAsyncDeveRetornarEntidadeValidaMesmoSemEncontrarId()
    {

        var faturaFinded = await _faturaRepository.FindAsync(keyValues: Guid.NewGuid().ToString());

        Assert.Null(faturaFinded);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task FindAsyncComPredicateDeveRetornarEntidadeValida()
    {

        var faturaFinded = await _faturaRepository.FindAsync(keyValues: _seedDbFixture.Fatura.Id);

        Assert.NotNull(faturaFinded);
        Assert.Equal(_seedDbFixture.Fatura.NumeroFatura, faturaFinded.NumeroFatura);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task FirstOrDefaultComPredicateDeveRetornarEntidadeValida()
    {

        var faturaFinded = await _faturaRepository.GetFirstOrDefaultAsync(predicate: x =>
            x.NumeroFatura == _seedDbFixture.Fatura.NumeroFatura);

        Assert.NotNull(faturaFinded);
        Assert.Equal(_seedDbFixture.Fatura.NumeroFatura, faturaFinded.NumeroFatura);
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task FirstOrDefaultComPredicateComIncludeDeveRetornarEntidadeValida()
    {

        const int npedido = 3;

        var fatura = _dataFixture.GerarFaturaFake().First();
        var pedidos = _dataFixture.GerarPedidoFake(npedido, 6);

        fatura.AdicionarPedido(pedidos);

        _ = await _faturaRepository.Add(fatura);
        _ = await _faturaRepository.SaveChangesAsync();

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

        var hoje = new DateTime(DateTime.Today.Year,
            DateTime.Today.Month,
            DateTime.Today.Day);

        var faturasFinded = await _faturaRepository.GetPagedListAsync(
            predicate: x => x.NumeroFatura == _seedDbFixture.Fatura.NumeroFatura &&
            x.DataCadastro >= hoje,
            disableTracking: false,
            ignoreQueryFilters: true);

        Assert.NotNull(faturasFinded);
        Assert.True(faturasFinded.Items.NotNullOrZero());
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task GetListComSelectorDeveRetornarEntidadeQueryResultValida()
    {

        var hoje = new DateTime(DateTime.Today.Year,
            DateTime.Today.Month,
            DateTime.Today.Day);

        var faturaQueryResult = await _faturaRepository.GetAllAsync(
            selector: s => new FaturaQueryResult { NumeroFatura = s.NumeroFatura, EntregaCidade = s.EnderecoEntrega.Cidade },
            predicate: x => x.NumeroFatura == _seedDbFixture.Fatura.NumeroFatura &&
            x.DataCadastro >= hoje,
            disableTracking: false,
            ignoreQueryFilters: true);

        Assert.True(faturaQueryResult.NotNullOrZero());
    }

    [SqlServerTestFact]
    [Trait("SqlServer", "Fatura Repository - ReadOnly")]
    public async Task GetListSemSelectorDeveRetornarEntidadeDeDominioValida()
    {

        _dbContext.PreventDisposal = false;

        var hoje = new DateTime(DateTime.Today.Year,
            DateTime.Today.Month,
            DateTime.Today.Day);

        var fatura = await _faturaRepository.GetAllAsync(
            predicate: x => x.NumeroFatura == _seedDbFixture.Fatura.NumeroFatura &&
            x.DataCadastro >= hoje,
            disableTracking: false,
            ignoreQueryFilters: true);

        Assert.True(fatura.NotNullOrZero());
    }

}
