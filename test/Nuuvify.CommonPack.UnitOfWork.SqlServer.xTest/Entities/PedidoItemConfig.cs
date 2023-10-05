using Nuuvify.CommonPack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Entities.StubDbContext
{
    public class PedidoItemConfig : EntityConfiguration<PedidoItem>
    {

        public override void Configure(EntityTypeBuilder<PedidoItem> builder)
        {

            DefaultConfig(builder, "PEDIDO_ITENS", "PEDIDO_ITEM");

            builder.Property(e => e.CodigoMercadoria)
                .IsRequired()
                .HasColumnType("varchar(10)");

            builder.Property(e => e.Quantidade)
                .IsRequired()
                .HasColumnName($"Qtd")
                .HasColumnType("numeric(10,4)");

            builder.Property(e => e.ValorUnitario)
                .IsRequired()
                .HasColumnType("decimal(18,4)");

            AuditConfig(builder);
            AuditUserIdConfig(builder);


            builder.Property(e => e.PedidoId)
                .IsRequired()
                .HasColumnName($"PedidoId")
                .HasColumnType($"varchar({DomainEntity.MaxId})");


            builder.HasOne(d => d.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(f => f.PedidoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PedidoItem_Pedido");

        }
    }
}
