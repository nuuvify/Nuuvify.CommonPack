using System;
using System.Linq;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;


namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Repositories
{
    [Collection(nameof(DataCollection))]
    public class FaturaRepository
    {
        private readonly AppDbContextFixture _dbContext;
        private readonly DataFixture _dataFixture;
        private readonly SeedDbFixture _seedDbFixture;
        private readonly ITestOutputHelper _outputHelper;


        private readonly Repository<Fatura> _faturaRepository;
        private const string UserRequest = "PostgreSQLUserTest";


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


            var uow = new UnitOfWork<DbContext>(_dbContext.Db)
            {
                UsernameContext = UserRequest
            };

            _faturaRepository = new Repository<Fatura>(_dbContext.Db, uow);

        }





        [PostgreSQLTestFact, Order(1)]
        [Trait("PostgreSQL", "Fatura Repository - Write")]
        public async Task DomainEvent_ComClassesIguaisComVersoesDiferentes_DeveGerarEventComAmbasVersoes()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(1) - Conex: {_dbContext.GetCnnStringToLog()}");


            _seedDbFixture.CreateData(
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

        [PostgreSQLTestFact, Order(2)]
        [Trait("PostgreSQL", "Fatura Repository - ReadOnly")]
        public async Task FindAsync_DeveRetornarEntidadeNotNull_MesmoSemEncontrarId()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(2) - Conex: {_dbContext.GetCnnStringToLog()}");


            var faturaFinded = await _faturaRepository.FindAsync(keyValues: Guid.NewGuid().ToString());


            Assert.Null(faturaFinded);
        }


        [PostgreSQLTestFact, Order(3)]
        [Trait("PostgreSQL", "Fatura Repository - Write")]
        public void FromSqlDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(3) - Conex: {_dbContext.GetCnnStringToLog()}");


            var sql = $"SELECT * FROM {_dbContext.Schema}.FATURAS WHERE numero_fatura = {_seedDbFixture.Fatura.NumeroFatura}";

            var faturaFinded = _faturaRepository.FromSql(sql).ToList();


            Assert.True(faturaFinded.Count > 0);
        }

        [PostgreSQLTestFact, Order(4)]
        [Trait("PostgreSQL", "Fatura Repository - ReadOnly")]
        public async Task FirstOrDefaultComPredicateDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(4) - Conex: {_dbContext.GetCnnStringToLog()}");


            var faturaFinded = await _faturaRepository.GetFirstOrDefaultAsync(predicate: x =>
                x.NumeroFatura == _seedDbFixture.Fatura.NumeroFatura,
                disableTracking: false);


            Assert.NotNull(faturaFinded);
            Assert.Equal(_seedDbFixture.Fatura.NumeroFatura, faturaFinded.NumeroFatura);
        }

        [PostgreSQLTestFact, Order(5)]
        [Trait("PostgreSQL", "Fatura Repository - ReadOnly")]
        public async Task FirstOrDefaultComPredicateComIncludeDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(5) - Conex: {_dbContext.GetCnnStringToLog()}");


            const int npedido = 3;

            var fatura = _dataFixture.GerarFaturaFake().First();
            var pedidos = _dataFixture.GerarPedidoFake(npedido, 6);

            fatura.AdicionarPedido(pedidos);

            await _faturaRepository.Add(fatura);
            await _faturaRepository.SaveChangesAsync();


            var numeroPedido = fatura.Pedidos.LastOrDefault().NumeroPedido;


            var faturaFinded = await _faturaRepository.GetFirstOrDefaultAsync(
                predicate: x => x.NumeroFatura == fatura.NumeroFatura,
                disableTracking: false);

            var faturaPedido = faturaFinded.Pedidos.FirstOrDefault(x => x.NumeroPedido == numeroPedido);

            Assert.NotNull(faturaFinded);
            Assert.Equal(numeroPedido, faturaPedido.NumeroPedido);
        }

        [PostgreSQLTestFact, Order(6)]
        [Trait("PostgreSQL", "Fatura Repository - ReadOnly")]
        public async Task GetListComPredicateDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(6) - Conex: {_dbContext.GetCnnStringToLog()}");


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

    }
}
