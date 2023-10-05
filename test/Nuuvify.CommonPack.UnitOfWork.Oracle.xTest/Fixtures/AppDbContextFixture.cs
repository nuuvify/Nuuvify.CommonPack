using System;
using System.Text;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Arrange;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Fixtures
{


    public class AppDbContextFixture : BaseAppDbContextFixture
    {

        private string RemoveTables { get; }
        public string Schema { get; }

        public AppDbContextFixture()
        {
            const string CnnTag = "B8CT2-Oracle";
            const string SchemaTag = "AppConfig:OwnerDB";
            const string CorrelationFake = "MinhaApp_4e1f0c64-f02e-435a-baa7-c78923ad371a_Oracle";


            PreventDisposal = true;
            IConfiguration config = AppSettingsConfig.GetConfig();
            var cnnString = config.GetConnectionString(CnnTag);
            Schema = config.GetSection(SchemaTag)?.Value;
            RemoveTables = config.GetSection("TestOptions:RemoveTables")?.Value;


            mockIConfigurationCustom = new Mock<IConfigurationCustom>();
            mockIConfigurationCustom.Setup(x => x.GetSectionValue(SchemaTag))
                .Returns(Schema);
            mockIConfigurationCustom.Setup(x => x.GetConnectionString(CnnTag))
                .Returns(cnnString);
            mockIConfigurationCustom.Setup(x => x.GetCorrelationId())
                .Returns(CorrelationFake);


            var options = new DbContextOptionsBuilder<StubDbContext>()
                .UseOracle(cnnString)
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
                    Console.WriteLine("Excluindo tabelas de teste...");

                    var delete = new StringBuilder("DELETE FROM ")
                        .AppendFormat("{0}.", Schema);


                    var sql = new StringBuilder()
                        .Append(delete)
                        .Append("PEDIDO_ITENS");

                    Db.Database.ExecuteSqlRaw(sql.ToString());

                    sql = new StringBuilder()
                        .Append(delete)
                        .Append("PEDIDOS");

                    Db.Database.ExecuteSqlRaw(sql.ToString());

                    sql = new StringBuilder()
                        .Append(delete)
                        .Append("FATURAS");

                    Db.Database.ExecuteSqlRaw(sql.ToString());

                    sql = new StringBuilder()
                        .Append(delete)
                        .Append("AUTOHISTORY");

                    Db.Database.ExecuteSqlRaw(sql.ToString());



                    Console.WriteLine("Tabelas de teste excluidas.");
                }
                else
                {
                    Console.WriteLine("Tabelas de teste não foram excluidas.");
                }

                Db.Dispose();
            }
        }

    }
}