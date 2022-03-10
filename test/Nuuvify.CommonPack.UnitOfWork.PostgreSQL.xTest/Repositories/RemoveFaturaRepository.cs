using System.Linq;
using System.Threading.Tasks;
using Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Repositories
{
    [Collection(nameof(DataCollection))]
    public class RemoveFaturaRepository
    {
        private readonly AppDbContextFixture _dbContext;
        private readonly DataFixture _dataFixture;
        private readonly SeedDbFixture _seedDbFixture;
        private readonly ITestOutputHelper _outputHelper;


        private readonly Repository<Fatura> _faturaRepository;
        private const string UserRequest = "PostgreSQLUserTest";


        public RemoveFaturaRepository(
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
        [Trait("PostgreSQL", "Remove Fatura Repository")]
        public async Task DomainEvent_ComClassesIguaisComVersoesDiferentes_DeveGerarEventComAmbasVersoes()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(1)");

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
        [Trait("PostgreSQL", "Remove Fatura Repository ")]
        public void FromSqlDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(2)");

            var sql = $"SELECT * FROM {_dbContext.Schema}.FATURAS WHERE NUMERO_FATURA = {_seedDbFixture.Fatura.NumeroFatura}";

            var faturaFinded = _faturaRepository.FromSql(sql).ToList();


            Assert.True(faturaFinded.Count > 0);
        }

        [PostgreSQLTestFact, Order(3)]
        [Trait("PostgreSQL", "Remove Fatura Repository")]
        public async Task RemoveFaturaPorIdNaoDeveRemoverCasoExistaPedido()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(3)");


            _dbContext.PreventDisposal = false;

            const string messageExpected = "null value in column \"fatura_id\" of relation \"pedidos\" violates not-null constraint";

            _faturaRepository.Remove(_seedDbFixture.Fatura.Id);

            var resultException = await Assert.ThrowsAsync<DbUpdateException>(()
                => _faturaRepository.SaveChangesAsync());


            Assert.Equal(UserRequest, _dbContext.Db.GetDbContextUsername());
            Assert.Contains(messageExpected, resultException.InnerException.Message);


        }
    }
}
