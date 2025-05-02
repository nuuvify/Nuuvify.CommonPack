using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nuuvify.CommonPack.Domain.Implementations;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Entities.StubDbContext;

public class PedidoItemConfig : EntityConfiguration<PedidoItem>
{

    public override void Configure(EntityTypeBuilder<PedidoItem> builder)
    {

        DefaultConfig(builder, "PEDIDO_ITENS", "PEDIDO_ITEM");

        _ = builder.Property(e => e.CodigoMercadoria)
            .IsRequired()
            .HasColumnType("varchar(10)");

        _ = builder.Property(e => e.Quantidade)
            .IsRequired()
            .HasColumnName($"Qtd")
            .HasColumnType("numeric(10,4)");

        _ = builder.Property(e => e.ValorUnitario)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        AuditConfig(builder);
        AuditUserIdConfig(builder);

        _ = builder.Property(e => e.PedidoId)
            .IsRequired()
            .HasColumnName($"PedidoId")
            .HasColumnType($"varchar({DomainEntity.MaxId})");

        _ = builder.HasOne(d => d.Pedido)
            .WithMany(p => p.Itens)
            .HasForeignKey(f => f.PedidoId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("FK_PedidoItem_Pedido");

    }
}
