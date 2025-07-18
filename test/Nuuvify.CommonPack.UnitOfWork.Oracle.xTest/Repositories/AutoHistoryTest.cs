using System.Threading.Tasks;
using Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Repositories
{
    [Collection(nameof(DataCollection))]
    public class AutoHistoryTest
    {
        private readonly AppDbContextFixture _dbContext;
        private readonly DataFixture _dataFixture;
        private readonly SeedDbFixture _seedDbFixture;
        private readonly ITestOutputHelper _outputHelper;
        private readonly Repository<Fatura> _faturaRepository;
        private const string UserRequest = "UoWOracleUserTest";


        public AutoHistoryTest(
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





        [OracleTestFact, Order(1)]
        [Trait("Oracle", "AutoHistory")]
        public async Task DomainEvent_ComAutoHistory_ComClassesIguaisComVersoesDiferentes_DeveGerarEventComAmbasVersoes()
        {
            const int RegistrosGravadosMaisHistory = 2;

            _seedDbFixture.CreateData(_outputHelper,
                _dbContext,
                _dataFixture,
                UserRequest,
                3, 2, true);



            var fatura = await _faturaRepository.GetFirstOrDefaultAsync(predicate:
                x => x.Id == _seedDbFixture.Fatura.Id);

            var faturaNewVersion = await _faturaRepository.FindAsync(fatura.Id);
            faturaNewVersion.Update("Essa é a versão 2 da fatura");


            var registries = await _faturaRepository.SaveChangesAsync(ensureAutoHistory: true);

            var faturaUpdated = new FaturaUpdatedEvent(fatura, "XYZ", faturaNewVersion);


            Assert.Equal(RegistrosGravadosMaisHistory, registries);
            Assert.True(faturaUpdated.SourceId.Equals(faturaUpdated.NewFatura));
            Assert.False(faturaUpdated.SourceId.Observacao == faturaUpdated.NewFatura.Observacao);
        }

        [OracleTestFact, Order(2)]
        [Trait("Oracle", "AutoHistory")]
        public async Task AutoHistory_ComCorrelationId_DeveGravarCampoCorrelationId()
        {
            _dbContext.PreventDisposal = false;

            const int RegistrosGravadosMaisHistory = 2;


            var fatura = await _faturaRepository.FindAsync(_seedDbFixture.Fatura.Id);
            fatura.Update("Testando correlationId no AutoHistory");


            var registries = await _faturaRepository.SaveChangesAsync(ensureAutoHistory: true);


            Assert.Equal(RegistrosGravadosMaisHistory, registries);


        }

  

    }
}
