using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Entities.StubDbContext
{
    public class FaturaConfig : EntityConfiguration<Fatura>
    {

        public override void Configure(EntityTypeBuilder<Fatura> builder)
        {


            // DefaultConfig(builder, "faturas", "fatura", "_id");
            //DefaultConfig(builder, "faturas", "fatura");

            builder.ToTable("faturas");

            builder.HasKey(x => x.Id)
                .HasName($"pk_fatura");

            builder.Property(x => x.Id)
                .HasColumnName($"FaturaId")
                .IsUnicode(false)
                .HasMaxLength(Fatura.MaxId)
                .IsRequired();


            builder.Property(e => e.NumeroFatura)
                .IsRequired()
                .HasColumnType("numeric(8)");


            builder.OwnsOne(
                o => o.EnderecoEntrega,
                pp =>
                {

                    pp.Property(p => p.Cidade)
                        .HasColumnName("EntregaCidade")
                        .IsUnicode(false)
                        .HasMaxLength(Endereco.MaxCidade);

                    pp.Property(p => p.Logradouro)
                        .HasColumnName("EntregaLogradouro")
                        .IsUnicode(false)
                        .HasMaxLength(Endereco.MaxLogradouro);

                });



            builder.OwnsOne(
                o => o.EnderecoFatura,
                pp =>
                {

                    pp.Property(p => p.Cidade)
                        .HasColumnName("FaturaCidade")
                        .IsUnicode(false)
                        .HasMaxLength(Endereco.MaxCidade);

                    pp.Property(p => p.Logradouro)
                        .HasColumnName("FaturaLogradouro")
                        .IsUnicode(false)
                        .HasMaxLength(Endereco.MaxLogradouro);

                });

            AuditConfig(builder);
            AuditUserIdIgnore(builder);


        }
    }
}
