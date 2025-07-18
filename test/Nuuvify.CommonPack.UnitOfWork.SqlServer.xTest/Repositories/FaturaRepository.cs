﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Extensions.Ordering;


namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Repositories
{
    [Collection(nameof(DataCollection))]
    public class FaturaRepository
    {
        private readonly AppDbContextFixture _dbContext;
        private readonly DataFixture _dataFixture;
        private readonly SeedDbFixture _seedDbFixture;
        private readonly ITestOutputHelper _outputHelper;


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
            _outputHelper = outputHelper;
            _seedDbFixture = seedDbFixture;


            var uow = new UnitOfWork<DbContext>(_dbContext.Db)
            {
                UsernameContext = UserRequest
            };

            _faturaRepository = new Repository<Fatura>(_dbContext.Db, uow);

        }





        [SqlServerTestFact, Order(1)]
        [Trait("SqlServer", "Fatura Repository - Write")]
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
        [Trait("SqlServer", "Fatura Repository - ReadOnly")]
        public async Task FindAsync_DeveRetornarEntidadeNotNull_MesmoSemEncontrarId()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(2)");

            var faturaFinded = await _faturaRepository.FindAsync(keyValues: Guid.NewGuid().ToString());


            Assert.Null(faturaFinded);
        }


        [SqlServerTestFact, Order(3)]
        [Trait("SqlServer", "Fatura Repository - Write")]
        public void FromSqlDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(3)");

            var sql = $"SELECT * FROM {_dbContext.Schema}.FATURAS WHERE NUMERO_FATURA = {_seedDbFixture.Fatura.NumeroFatura}";

            var faturaFinded = _faturaRepository.FromSql(sql).ToList();


            Assert.True(faturaFinded.Count > 0);
        }

        [SqlServerTestFact, Order(4)]
        [Trait("SqlServer", "Fatura Repository - ReadOnly")]
        public async Task FirstOrDefaultComPredicateDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(4)");

            var faturaFinded = await _faturaRepository.GetFirstOrDefaultAsync(predicate: x =>
                x.NumeroFatura == _seedDbFixture.Fatura.NumeroFatura,
                disableTracking: false);


            Assert.NotNull(faturaFinded);
            Assert.Equal(_seedDbFixture.Fatura.NumeroFatura, faturaFinded.NumeroFatura);
        }

        [SqlServerTestFact, Order(5)]
        [Trait("SqlServer", "Fatura Repository - ReadOnly")]
        public async Task FirstOrDefaultComPredicateComIncludeDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(5)");

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

        [SqlServerTestFact, Order(6)]
        [Trait("SqlServer", "Fatura Repository - ReadOnly")]
        public async Task GetListComPredicateDeveRetornarEntidadeValida()
        {
            _outputHelper.WriteLine($"{this.GetType().Name} - Order(6)");

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
