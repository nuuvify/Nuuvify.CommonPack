using Nuuvify.CommonPack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Entities.StubDbContext
{
    public class PedidoItemConfig : EntityConfiguration<PedidoItem>
    {

        public override void Configure(EntityTypeBuilder<PedidoItem> builder)
        {

            DefaultConfig(builder, "PEDIDO_ITENS", "PEDIDO_ITEM");


            builder.Property(e => e.CodigoMercadoria)
                .IsRequired()
                .HasColumnName($"CODIGO_MERCADORIA")
                .HasColumnType("VARCHAR2(10)");

            builder.Property(e => e.Quantidade)
                .IsRequired()
                .HasColumnName($"QTD")
                .HasColumnType("NUMBER(10,4)");

            builder.Property(e => e.ValorUnitario)
                .IsRequired()
                .HasColumnName($"VALOR_UNITARIO")
                .HasColumnType("NUMBER(18,4)");


            AuditConfig(builder);
            AuditUserIdConfig(builder);
            

            builder.Property(e => e.PedidoId)
                .IsRequired()
                .HasColumnName($"PEDIDO_ID")
                .HasColumnType($"VARCHAR2({DomainEntity.MaxId})");


            builder.HasOne(d => d.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(f => f.PedidoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PEDIDO_ITEM_PEDIDO");

        }
    }
}
