using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nuuvify.CommonPack.AutoHistory.Extensions;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Middleware.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Arrange
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


            try
            {
                if (!Database.EnsureCreated())
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


        public virtual DbSet<PedidoItem> PedidoItens { get; set; }
        public virtual DbSet<Pedido> Pedidos { get; set; }
        public virtual DbSet<Fatura> Faturas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {

                var cnn = Configuration.GetConnectionString(ownerDB);

                if (Database.ProviderName.Contains("SqlServer", StringComparison.OrdinalIgnoreCase))
                {
                    optionsBuilder
                        .UseSqlServer(cnn)
                        .UseLazyLoadingProxies()
                        .EnableDetailedErrors()
                        .EnableSensitiveDataLogging();

                    Console.WriteLine($"EF OnConfiguring: {cnn.SubstringNotNull(0, cnn.IndexOf("User"))}");

                }
                else
                {

                    optionsBuilder
                        .UseLazyLoadingProxies()
                        .EnableDetailedErrors()
                        .EnableSensitiveDataLogging()
                        .UseInMemoryDatabase(ownerDB);

                    Console.WriteLine($"EF OnConfiguring: InMemory");
                }


            }

        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.SetDatabaseProviderName(Database);

            modelBuilder.HasDefaultSchema(ownerDB);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(StubDbContext).Assembly);

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
            catch (DbUpdateException ex)
            {
                PropertyValues proposedValues;
                PropertyValues databaseValues;
                var columnName = string.Empty;

                var baseMessage = new StringBuilder()
                    .AppendLine($"Houve um erro em SaveChanges, verifique o log de erros: {ex.Message} inner: {ex?.InnerException?.Message}");

                foreach (var entry in ex.Entries)
                {
                    proposedValues = entry.CurrentValues;
                    databaseValues = entry.GetDatabaseValues();

                    foreach (var property in proposedValues.Properties)
                    {
                        columnName = property.GetColumnName();

                        baseMessage.AppendLine($"Proposed: {columnName} = {proposedValues[property]}");
                        if (!(databaseValues?[property] is null))
                        {
                            baseMessage.AppendLine($"DataBaseValue: {columnName} = {databaseValues?[property]}");
                        }
                    }

                }

                Debug.WriteLine($"{baseMessage}");
                throw;

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message} inner: {ex?.InnerException?.Message}");
                throw;
            }

        }


    }

}