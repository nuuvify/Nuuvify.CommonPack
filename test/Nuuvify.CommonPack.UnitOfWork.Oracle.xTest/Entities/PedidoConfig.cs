using Nuuvify.CommonPack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Entities.StubDbContext
{
    public class PedidoConfig : EntityConfiguration<Pedido>
    {

        public override void Configure(EntityTypeBuilder<Pedido> builder)
        {


            DefaultConfig(builder, "PEDIDOS", "PEDIDO");

            builder.Property(e => e.CodigoCliente)
                .IsRequired()
                .HasColumnName($"CODIGO_CLIENTE")
                .HasColumnType("VARCHAR2(10)");

            builder.Property(e => e.NumeroPedido)
                .IsRequired()
                .HasColumnName($"NUMERO_PEDIDO")
                .HasColumnType("NUMBER(8)");

            builder.Property(e => e.DataPedido)
                .IsRequired()
                .HasColumnName($"DATA_PEDIDO");


            AuditConfig(builder);
            AuditUserIdConfig(builder);


            builder.Property(e => e.FaturaId)
                .IsRequired()
                .HasColumnName($"FATURA_ID")
                .HasColumnType($"VARCHAR2({DomainEntity.MaxId})");


            builder.HasOne(d => d.FaturaPedido)
                .WithMany(p => p.Pedidos)
                .HasForeignKey(f => f.FaturaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_PEDIDO_FATURA");


        }
    }
}
