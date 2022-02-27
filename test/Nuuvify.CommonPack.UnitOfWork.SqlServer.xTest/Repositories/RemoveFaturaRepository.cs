using System.Linq;
using System.Threading.Tasks;
using Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Repositories
{
    [Collection(nameof(DataCollection))]
    public class RemoveFaturaRepository
    {
        private readonly AppDbContextFixture _dbContext;
        private readonly DataFixture _dataFixture;
        private readonly SeedDbFixture _seedDbFixture;
        private readonly ITestOutputHelper _outputHelper;


        private readonly Repository<Fatura> _faturaRepository;
        private const string UserRequest = "SqlServerUserTest";


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





        [SqlServerTestFact, Order(1)]
        [Trait("SqlServer", "Remove Fatura Repository")]
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


        [SqlServerTestFact, Order(2)]
        [Trait("SqlServer", "Remove Fatura Repository ")]
        public void FromSqlDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(2)");

            var sql = $"SELECT * FROM {_dbContext.Schema}.FATURAS WHERE NUMERO_FATURA = {_seedDbFixture.Fatura.NumeroFatura}";

            var faturaFinded = _faturaRepository.FromSql(sql).ToList();


            Assert.True(faturaFinded.Count > 0);
        }

        [SqlServerTestFact, Order(3)]
        [Trait("SqlServer", "Remove Fatura Repository")]
        public async Task RemoveFaturaPorIdNaoDeveRemoverCasoExistaPedido()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(3)");


            _dbContext.PreventDisposal = false;

            const string messageExpected = "Cannot insert the value NULL into column 'FaturaId', table '.b8ct2.PEDIDOS'; column does not allow nulls. UPDATE fails";

            _faturaRepository.Remove(_seedDbFixture.Fatura.Id);

            var resultException = await Assert.ThrowsAsync<DbUpdateException>(()
                => _faturaRepository.SaveChangesAsync());


            Assert.Equal(UserRequest, _dbContext.Db.GetDbContextUsername());
            Assert.Contains(messageExpected, resultException.InnerException.Message);


        }
    }
}
