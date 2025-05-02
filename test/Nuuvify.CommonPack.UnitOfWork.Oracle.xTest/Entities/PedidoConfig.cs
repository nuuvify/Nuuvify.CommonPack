using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nuuvify.CommonPack.Domain.Implementations;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Entities.StubDbContext;

public class PedidoConfig : EntityConfiguration<Pedido>
{

    public override void Configure(EntityTypeBuilder<Pedido> builder)
    {

        DefaultConfig(builder, "PEDIDOS", "PEDIDO");

        _ = builder.Property(e => e.CodigoCliente)
            .IsRequired()
            .HasColumnName($"CODIGO_CLIENTE")
            .HasColumnType("VARCHAR2(10)");

        _ = builder.Property(e => e.NumeroPedido)
            .IsRequired()
            .HasColumnName($"NUMERO_PEDIDO")
            .HasColumnType("NUMBER(8)");

        _ = builder.Property(e => e.DataPedido)
            .IsRequired()
            .HasColumnName($"DATA_PEDIDO");

        AuditConfig(builder);
        AuditUserIdConfig(builder);

        _ = builder.Property(e => e.FaturaId)
            .IsRequired()
            .HasColumnName($"FATURA_ID")
            .HasColumnType($"VARCHAR2({DomainEntity.MaxId})");

        _ = builder.HasOne(d => d.FaturaPedido)
            .WithMany(p => p.Pedidos)
            .HasForeignKey(f => f.FaturaId)
            .OnDelete(DeleteBehavior.Restrict)
            .HasConstraintName("FK_PEDIDO_FATURA");

    }
}
