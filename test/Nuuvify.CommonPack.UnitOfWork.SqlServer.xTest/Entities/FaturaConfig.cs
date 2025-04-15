using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.SqlServer.xTest.Entities.StubDbContext;

public class FaturaConfig : EntityConfiguration<Fatura>
{

    public override void Configure(EntityTypeBuilder<Fatura> builder)
    {

        DefaultConfig(builder, "FATURAS", "FATURA");

        _ = builder.Property(e => e.NumeroFatura)
            .IsRequired()
            .HasColumnName($"NUMERO_FATURA")
            .HasColumnType("NUMERIC(8)");

        _ = builder.OwnsOne(
            o => o.EnderecoEntrega,
            pp =>
            {
                _ = pp.Property(p => p.Cidade)
                    .HasColumnName("EntregaCidade")
                    .HasColumnType($"varchar({Endereco.MaxCidade})");

                _ = pp.Property(p => p.Logradouro)
                    .HasColumnName("EntregaLogradouro")
                    .HasColumnType($"varchar({Endereco.MaxLogradouro})");

            });

        _ = builder.OwnsOne(
            o => o.EnderecoFatura,
            pp =>
            {
                _ = pp.Property(p => p.Cidade)
                    .HasColumnName("FaturaCidade")
                    .HasColumnType($"varchar({Endereco.MaxCidade})");

                _ = pp.Property(p => p.Logradouro)
                    .HasColumnName("FaturaLogradouro")
                    .HasColumnType($"varchar({Endereco.MaxLogradouro})");

            });

        AuditConfig(builder);
        AuditUserIdIgnore(builder);

    }
}
