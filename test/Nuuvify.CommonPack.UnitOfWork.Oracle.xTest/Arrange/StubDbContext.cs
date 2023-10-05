using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nuuvify.CommonPack.AutoHistory.Extensions;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Middleware.Abstraction;
using EntityFramework.Exceptions.Common;
using EntityFramework.Exceptions.Oracle;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Arrange
{
    public class StubDbContext : DbContext
    {

        public readonly IConfigurationCustom Configuration;
        public readonly string ownerDB;



        public StubDbContext(DbContextOptions<StubDbContext> options,
            IConfigurationCustom configuration)
            : base(options)
        {

            Configuration = configuration;

            ownerDB = Configuration.GetSectionValue("AppConfig:OwnerDB");

            if (ownerDB != null)
            {
                var schemas = new string[] { ownerDB };
                try
                {
                    if (!Database.EnsureCreated(schemas))
                    {
                        var databaseCreator = Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;

                        if (!databaseCreator.HasTables())
                            databaseCreator.CreateTables();
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Não foi possivel criar as tabelas para teste, owner: {ownerDB} para execução do teste. {ex.Message}");
                }
            }

        }


        public virtual DbSet<PedidoItem> PedidoItens { get; set; }
        public virtual DbSet<Pedido> Pedidos { get; set; }
        public virtual DbSet<Fatura> Faturas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                var cnn = Configuration.GetConnectionString(ownerDB);

                optionsBuilder
                   .UseLazyLoadingProxies()
                   .EnableDetailedErrors()
                   .EnableSensitiveDataLogging()
                   .UseOracle(cnn);

                Console.WriteLine($"EF OnConfiguring: {cnn.SubstringNotNull(0, cnn.IndexOf("User"))}");

            }

            optionsBuilder.UseExceptionProcessor();

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetDatabaseProviderName(Database);

            modelBuilder.HasDefaultSchema(ownerDB);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StubDbContext).Assembly);
            // modelBuilder.ApplyConfigurationsFromAssembly(typeof(StubDbContext).Assembly,
            //     predicate: n => n.Namespace.EndsWith(nameof(StubDbContext)));

            modelBuilder.IgnoreValueObject();

            modelBuilder.MappingPropertiesForgotten();



            modelBuilder.EnableAutoHistory<Nuuvify.CommonPack.AutoHistory.AutoHistory>(o =>
            {
                o.ProviderName = Database.ProviderName;
            });


            base.OnModelCreating(modelBuilder);

        }


        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var registries = await SaveChangesAsync(true, cancellationToken);

                Debug.WriteLine($"SaveChanges executado com sucesso para {registries} registros, e {this.GetAggregatesChanges()} registros em entidades agregadas");

                return await Task.FromResult(registries);
            }
            catch (CustomException ex)
            {
                Debug.WriteLine($"O conteudo da exception deve ser consultada no log da aplicação: {ex.Message} Inner: {ex?.InnerException?.Message}");
                throw;
            }
            catch (CustomDbUpdateException ex)
            {
                Debug.WriteLine($"{ex.Message}");

                foreach (var message in ex.CustomMessage())
                {
                    Debug.WriteLine($"{message.Key} {message.Value}");
                }
                throw;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"O conteudo da exception deve ser consultada no log da aplicação: {ex.Message} Inner: {ex?.InnerException?.Message}");
                throw;
            }

        }
    }
}
