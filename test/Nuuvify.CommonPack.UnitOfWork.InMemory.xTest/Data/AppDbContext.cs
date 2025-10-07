using Microsoft.EntityFrameworkCore;
using AutoHistoryEntity = Nuuvify.CommonPack.AutoHistory.AutoHistory;
using Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Entities;

namespace Nuuvify.CommonPack.UnitOfWork.InMemory.xTest.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Fatura> Faturas => Set<Fatura>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<PedidoItem> PedidoItens => Set<PedidoItem>();
    public DbSet<AutoHistoryEntity> AutoHistories => Set<AutoHistoryEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Ignore value objects that implement INotPersistingAsTable
        modelBuilder.IgnoreValueObject();
        // Configure Endereco as Owned Entity for Fatura
        _ = modelBuilder.Entity<Fatura>()
            .OwnsOne(f => f.EnderecoFatura, endereco =>
            {
                _ = endereco.Property(e => e.Logradouro).HasColumnName("EnderecoFatura_Logradouro");
                _ = endereco.Property(e => e.Numero).HasColumnName("EnderecoFatura_Numero");
                _ = endereco.Property(e => e.Cidade).HasColumnName("EnderecoFatura_Cidade");
                _ = endereco.Property(e => e.Cep).HasColumnName("EnderecoFatura_Cep");
            });

        _ = modelBuilder.Entity<Fatura>()
            .OwnsOne(f => f.EnderecoEntrega, endereco =>
            {
                _ = endereco.Property(e => e.Logradouro).HasColumnName("EnderecoEntrega_Logradouro");
                _ = endereco.Property(e => e.Numero).HasColumnName("EnderecoEntrega_Numero");
                _ = endereco.Property(e => e.Cidade).HasColumnName("EnderecoEntrega_Cidade");
                _ = endereco.Property(e => e.Cep).HasColumnName("EnderecoEntrega_Cep");
            });

        // Configure relationships
        _ = modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Fatura)
            .WithMany(f => f.Pedidos)
            .HasForeignKey("FaturaId")
            .IsRequired(false);

        _ = modelBuilder.Entity<PedidoItem>()
            .HasOne(pi => pi.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey("PedidoId")
            .IsRequired(false);

        // Configure AutoHistory
        _ = modelBuilder.Entity<AutoHistoryEntity>(entity =>
        {
            _ = entity.HasKey(e => e.Id);
            _ = entity.Property(e => e.TableName).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
