using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Entities.StubDbContext
{
    public class FaturaConfig : EntityConfiguration<Fatura>
    {

        public override void Configure(EntityTypeBuilder<Fatura> builder)
        {


            DefaultConfig(builder, "faturas", "fatura", "_id");

            builder.Property(e => e.NumeroFatura)
                .IsRequired()
                .HasColumnType("NUMERIC(8)");


            builder.OwnsOne(
                o => o.EnderecoEntrega,
                pp =>
                {
                    pp.Property(p => p.Cidade)
                        .HasColumnName("EntregaCidade")
                        .HasMaxLength(Endereco.MaxCidade);

                    pp.Property(p => p.Logradouro)
                        .HasColumnName("EntregaLogradouro")
                        .HasMaxLength(Endereco.MaxLogradouro);

                });

            builder.OwnsOne(
                o => o.EnderecoFatura,
                pp =>
                {
                    pp.Property(p => p.Cidade)
                        .HasColumnName("FaturaCidade")
                        .HasMaxLength(Endereco.MaxCidade);

                    pp.Property(p => p.Logradouro)
                        .HasColumnName("FaturaLogradouro")
                        .HasMaxLength(Endereco.MaxLogradouro);

                });

            AuditConfig(builder);
            AuditUserIdIgnore(builder);


        }
    }
}
