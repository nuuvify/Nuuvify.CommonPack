using System;
using System.Linq;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Repositories
{
    [Collection(nameof(DataCollection))]
    public class FaturaRepositoryReadOnly
    {
        private readonly AppDbContextFixture _dbContext;
        private readonly DataFixture _dataFixture;
        private readonly SeedDbFixture _seedDbFixture;
        private readonly ITestOutputHelper _outputHelper;


        private readonly RepositoryReadOnly<Fatura> _faturaRepository;
        private const string UserRequest = "OracleUserTest";


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



            _faturaRepository = new RepositoryReadOnly<Fatura>(_dbContext.Db);

        }



        // [OracleTestFact, Order(1)]
        // [Trait("Oracle","Fatura Repository - ReadOnly")]
        // public async Task FindAsyncDeveRetornarEntidadeValida()
        // {

        //     _seedDbFixture.CreateData(_outputHelper,
        //         _dbContext,
        //         _dataFixture,
        //         UserRequest,
        //         1, 2, true);


        //     var faturaFinded = await _faturaRepository.FindAsync(keyValues: _seedDbFixture.Fatura.Id);


        //     Assert.NotNull(faturaFinded);
        // }

        [OracleTestFact, Order(2)]
        [Trait("Oracle", "Fatura Repository - ReadOnly")]
        public async Task FindAsyncDeveRetornarEntidadeValidaMesmoSemEncontrarId()
        {
            _dbContext.PreventDisposal = false;
            
            var faturaFinded = await _faturaRepository.FindAsync(keyValues: Guid.NewGuid().ToString());


            Assert.Null(faturaFinded);
        }

        [OracleTestFact, Order(3)]
        [Trait("Oracle", "Fatura Repository - ReadOnly")]
        public async Task FindAsyncComPredicateDeveRetornarEntidadeValida()
        {
            var faturaFinded = await _faturaRepository.FindAsync(_seedDbFixture.Fatura.Id);


            Assert.NotNull(faturaFinded);
            Assert.Equal(_seedDbFixture.Fatura.NumeroFatura, faturaFinded.NumeroFatura);
        }


        [OracleTestFact, Order(4)]
        [Trait("Oracle", "Fatura Repository - ReadOnly")]
        public async Task FirstOrDefaultComPredicateDeveRetornarEntidadeValida()
        {

            var faturaFinded = await _faturaRepository.GetFirstOrDefaultAsync(predicate: x =>
                x.NumeroFatura == _seedDbFixture.Fatura.NumeroFatura);


            Assert.NotNull(faturaFinded);
            Assert.Equal(_seedDbFixture.Fatura.NumeroFatura, faturaFinded.NumeroFatura);
        }




        [OracleTestFact, Order(5)]
        [Trait("Oracle", "Fatura Repository - ReadOnly")]
        public async Task GetListComPredicateDeveRetornarEntidadeValida()
        {
            _dbContext.PreventDisposal = false;

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
