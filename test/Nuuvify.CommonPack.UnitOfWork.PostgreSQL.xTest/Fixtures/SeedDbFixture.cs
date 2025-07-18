using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Fixtures
{

    public class SeedDbFixture
    {
        public Fatura Fatura { get; private set; }

        public void CreateData(
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

            Debug.WriteLine($"Registros incluidos para teste: {registries}");

        }
    }
}