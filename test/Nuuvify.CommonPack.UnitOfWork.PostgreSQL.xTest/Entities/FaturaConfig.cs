using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.PostgreSQL.xTest.Entities.StubDbContext;

public class FaturaConfig : EntityConfiguration<Fatura>
{

    public override void Configure(EntityTypeBuilder<Fatura> builder)
    {

        // DefaultConfig(builder, "faturas", "fatura", "_id");
        //DefaultConfig(builder, "faturas", "fatura");

        _ = builder.ToTable("faturas");

        _ = builder.HasKey(x => x.Id)
            .HasName($"pk_fatura");

        _ = builder.Property(x => x.Id)
            .HasColumnName($"FaturaId")
            .IsUnicode(false)
            .HasMaxLength(Fatura.MaxId)
            .IsRequired();

        _ = builder.Property(e => e.NumeroFatura)
            .IsRequired()
            .HasColumnType("numeric(8)");

        _ = builder.OwnsOne(
            o => o.EnderecoEntrega,
            pp =>
            {

                _ = pp.Property(p => p.Cidade)
                    .HasColumnName("EntregaCidade")
                    .IsUnicode(false)
                    .HasMaxLength(Endereco.MaxCidade);

                _ = pp.Property(p => p.Logradouro)
                    .HasColumnName("EntregaLogradouro")
                    .IsUnicode(false)
                    .HasMaxLength(Endereco.MaxLogradouro);

            });

        _ = builder.OwnsOne(
            o => o.EnderecoFatura,
            pp =>
            {

                _ = pp.Property(p => p.Cidade)
                    .HasColumnName("FaturaCidade")
                    .IsUnicode(false)
                    .HasMaxLength(Endereco.MaxCidade);

                _ = pp.Property(p => p.Logradouro)
                    .HasColumnName("FaturaLogradouro")
                    .IsUnicode(false)
                    .HasMaxLength(Endereco.MaxLogradouro);

            });

        AuditConfig(builder);
        AuditUserIdIgnore(builder);

    }
}
