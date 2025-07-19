using Nuuvify.CommonPack.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Entities.StubDbContext
{
    public class PedidoConfig : EntityConfiguration<Pedido>
    {

        public override void Configure(EntityTypeBuilder<Pedido> builder)
        {

            DefaultConfig(builder, "PEDIDOS", "PEDIDO");

            builder.Property(e => e.CodigoCliente)
                .IsRequired()
                .HasColumnType("varchar(10)");

            builder.Property(e => e.NumeroPedido)
                .IsRequired()
                .HasColumnType("numeric(8)");

            builder.Property(e => e.DataPedido)
                .IsRequired();

            AuditConfig(builder);
            AuditUserIdConfig(builder);


            builder.Property(e => e.FaturaId)
                .IsRequired()
                .HasColumnName($"FaturaId")
                .HasColumnType($"varchar({DomainEntity.MaxId})");


            builder.HasOne(d => d.FaturaPedido)
                .WithMany(p => p.Pedidos)
                .HasForeignKey(f => f.FaturaId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Pedido_Fatura");


        }
    }
}
