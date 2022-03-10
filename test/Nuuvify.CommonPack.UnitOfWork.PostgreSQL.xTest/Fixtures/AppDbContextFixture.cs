using System;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Arrange;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Fixtures
{


    public class AppDbContextFixture : BaseAppDbContextFixture
    {

        private string RemoveTables { get; set; }
        public string Schema { get; }
        private string CnnString { get; set; }


        public AppDbContextFixture()
        {

            const string CnnTag = "vendas";
            const string SchemaTag = "AppConfig:OwnerDB";
            const string CorrelationFake = "MinhaApp_4e1f0c64-f02e-435a-baa7-c78923ad371a_PostgreSQL";

            PreventDisposal = true;
            IConfiguration config = AppSettingsConfig.GetConfig();

            var cnnStringEnv = Environment.GetEnvironmentVariable("PostgreVendas".ToUpper());
            Console.WriteLine($"String Conex: {GetCnnStringToLog(cnnStringEnv)}");

            CnnString = config.GetConnectionString(CnnTag);
            CnnString = string.IsNullOrWhiteSpace(CnnString) ? cnnStringEnv : CnnString;

            Schema = config.GetSection(SchemaTag)?.Value;
            RemoveTables = config.GetSection("TestOptions:RemoveTables")?.Value;


            mockIConfigurationCustom = new Mock<IConfigurationCustom>();
            mockIConfigurationCustom.Setup(x => x.GetSectionValue(SchemaTag))
                .Returns(Schema);
            mockIConfigurationCustom.Setup(x => x.GetConnectionString(CnnTag))
                .Returns(CnnString);
            mockIConfigurationCustom.Setup(x => x.GetCorrelationId())
                .Returns(CorrelationFake);

            var options = new DbContextOptionsBuilder<StubDbContext>()
                .UseNpgsql(CnnString)
                .UseSnakeCaseNamingConvention()
                .UseLazyLoadingProxies()
                .EnableDetailedErrors()
                .EnableSensitiveDataLogging()
                .Options;


            Db = new StubDbContext(options, mockIConfigurationCustom.Object);
        }

        public string GetCnnStringToLog(string stringToLog = null)
        {
            if (string.IsNullOrWhiteSpace(stringToLog))
            {
                stringToLog = CnnString;
            }
            if (string.IsNullOrWhiteSpace(stringToLog)) return "";

            var lenCnn = stringToLog.Length;

            if (lenCnn >= 15)
            {
                return stringToLog.Substring(0, 15);
            }

            return stringToLog.Substring(0, 3);

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
                        .Append("autohistory");

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