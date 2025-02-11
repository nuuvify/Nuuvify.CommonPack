using Nuuvify.CommonPack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Entities.StubDbContext
{
    public class PedidoItemConfig : EntityConfiguration<PedidoItem>
    {

        public override void Configure(EntityTypeBuilder<PedidoItem> builder)
        {

            DefaultConfig(builder, "pedido_itens", "pedido_item");

            builder.Property(e => e.CodigoMercadoria)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(10);

            builder.Property(e => e.Quantidade)
                .IsRequired()
                .HasColumnName($"qtd")
                .HasColumnType("numeric(10,4)");

            builder.Property(e => e.ValorUnitario)
                .IsRequired()
                .HasColumnType("decimal(18,4)");


            builder.Property(e => e.PedidoId)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxId);


            builder.HasOne(d => d.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(f => f.PedidoId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_PedidoItem_Pedido");

            AuditConfig(builder);
            AuditUserIdConfig(builder);
        }
    }
}
