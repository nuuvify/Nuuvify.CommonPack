using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Arrange;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Fixtures;


public class AppDbContextFixture : BaseAppDbContextFixture
{

    private string RemoveTables { get; }
    public string Schema { get; }

    public AppDbContextFixture()
    {
        const string CnnTag = "Vendas";
        const string SchemaTag = "AppConfig:OwnerDB";
        const string CorrelationFake = "MinhaApp_4e1f0c64-f02e-435a-baa7-c78923ad371a_SqlServer";

        PreventDisposal = true;
        IConfiguration config = AppSettingsConfig.GetConfig();
        var cnnString = config.GetConnectionString(CnnTag);
        Schema = config.GetSection(SchemaTag)?.Value;
        RemoveTables = config.GetSection("TestOptions:RemoveTables")?.Value;

        mockIConfigurationCustom = new Mock<IConfigurationCustom>();
        _ = mockIConfigurationCustom.Setup(x => x.GetSectionValue(SchemaTag))
            .Returns(Schema);
        _ = mockIConfigurationCustom.Setup(x => x.GetConnectionString(CnnTag))
            .Returns(cnnString);
        _ = mockIConfigurationCustom.Setup(x => x.GetCorrelationId())
            .Returns(CorrelationFake);

        var options = new DbContextOptionsBuilder<StubDbContext>()
            .UseSqlServer(cnnString)
            .EnableDetailedErrors()
            .EnableSensitiveDataLogging()
            .UseLazyLoadingProxies()
            .Options;

        Db = new StubDbContext(options, mockIConfigurationCustom.Object);

    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && Db != null && !PreventDisposal)
        {
            if (!Db.Database.IsInMemory() && RemoveTables.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine("Excluindo tabelas de teste...");

                var delete = new StringBuilder("DELETE FROM ")
                    .AppendFormat("{0}.", Schema);

                var sql = new StringBuilder()
                    .Append(delete)
                    .Append("PEDIDO_ITENS");

                _ = Db.Database.ExecuteSqlRaw(sql.ToString());

                sql = new StringBuilder()
                    .Append(delete)
                    .Append("PEDIDOS");

                _ = Db.Database.ExecuteSqlRaw(sql.ToString());

                sql = new StringBuilder()
                    .Append(delete)
                    .Append("FATURAS");

                _ = Db.Database.ExecuteSqlRaw(sql.ToString());

                sql = new StringBuilder()
                    .Append(delete)
                    .Append("AUTOHISTORY");

                _ = Db.Database.ExecuteSqlRaw(sql.ToString());

                Debug.WriteLine("Tabelas de teste excluidas.");
            }
            else
            {
                Debug.WriteLine("Tabelas de teste não foram excluidas.");
            }

            Db.Dispose();
        }
    }

}
