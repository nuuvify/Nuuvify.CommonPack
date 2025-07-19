using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Entities.StubDbContext
{
    public class FaturaConfig : EntityConfiguration<Fatura>
    {

        public override void Configure(EntityTypeBuilder<Fatura> builder)
        {


            DefaultConfig(builder, "FATURAS", "FATURA");

            builder.Property(e => e.NumeroFatura)
                .IsRequired()
                .HasColumnName($"NUMERO_FATURA")
                .HasColumnType("NUMERIC(8)");


            builder.OwnsOne(
                o => o.EnderecoEntrega,
                pp =>
                {
                    pp.Property(p => p.Cidade)
                        .HasColumnName("EntregaCidade")
                        .HasColumnType($"varchar({Endereco.MaxCidade})");

                    pp.Property(p => p.Logradouro)
                        .HasColumnName("EntregaLogradouro")
                        .HasColumnType($"varchar({Endereco.MaxLogradouro})");

                });

            builder.OwnsOne(
                o => o.EnderecoFatura,
                pp =>
                {
                    pp.Property(p => p.Cidade)
                        .HasColumnName("FaturaCidade")
                        .HasColumnType($"varchar({Endereco.MaxCidade})");

                    pp.Property(p => p.Logradouro)
                        .HasColumnName("FaturaLogradouro")
                        .HasColumnType($"varchar({Endereco.MaxLogradouro})");

                });


                AuditConfig(builder);
                AuditUserIdIgnore(builder);


        }
    }
}
