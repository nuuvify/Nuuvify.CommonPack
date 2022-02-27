using System.Linq;
using Microsoft.EntityFrameworkCore;
using Xunit.Abstractions;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Fixtures
{

    public class SeedDbFixture
    {
        public Fatura Fatura { get; private set; }



        public void CreateData(ITestOutputHelper outputHelper,
            BaseAppDbContextFixture dbContextFixture,
            DataFixture dataFixture,
            string usertest, int pedidos, int itens, bool saveDb = false)
        {

            Fatura = dataFixture.GerarFaturaFake().First();

            var pedido = dataFixture.GerarPedidoFake(pedidos, itens);

            for (int p = 0; p < pedido.Count; p++)
            {
                Fatura.AdicionarPedido(pedido[p]);
            }



            dbContextFixture.Db.SetDbContextUsername(usertest);

            dbContextFixture.Db.Add(Fatura);

            var registries = 0;
            if (saveDb)
            {
                var uow = new UnitOfWork<DbContext>(dbContextFixture.Db)
                {
                    UsernameContext = usertest
                };
                registries = uow.SaveChangesAsync().Result;
            }

            outputHelper.WriteLine("Registros incluidos para teste {0}", registries);

        }
    }
}