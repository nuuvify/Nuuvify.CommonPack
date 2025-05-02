using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nuuvify.CommonPack.Domain.Implementations;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Entities.StubDbContext;

public class PedidoConfig : EntityConfiguration<Pedido>
{

    public override void Configure(EntityTypeBuilder<Pedido> builder)
    {

        // DefaultConfig(builder, "pedidos", "pedido", "_id");
        // DefaultConfig(builder, "pedidos", "pedido");

        _ = builder.ToTable("pedidos");

        _ = builder.HasKey(x => x.Id)
            .HasName($"pk_pedido");

        _ = builder.Property(x => x.Id)
            .HasColumnName($"PedidoId")
            .IsUnicode(false)
            .HasMaxLength(Pedido.MaxId)
            .IsRequired();

        _ = builder.Property(e => e.CodigoCliente)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(10);

        _ = builder.Property(e => e.NumeroPedido)
            .IsRequired()
            .HasColumnType("numeric(8)");

        _ = builder.Property(e => e.DataPedido)
            .IsRequired();

        _ = builder.Property(e => e.FaturaId)
            .IsRequired()
            .IsUnicode(false)
            .HasMaxLength(DomainEntity.MaxId);

        _ = builder.HasOne(d => d.FaturaPedido)
            .WithMany(p => p.Pedidos)
            .HasForeignKey(f => f.FaturaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_Pedido_Fatura");

        AuditConfig(builder);
        AuditUserIdConfig(builder);

    }
}
