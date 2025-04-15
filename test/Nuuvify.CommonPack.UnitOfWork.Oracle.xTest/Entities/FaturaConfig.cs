using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Entities.StubDbContext;

public class FaturaConfig : EntityConfiguration<Fatura>
{

    public override void Configure(EntityTypeBuilder<Fatura> builder)
    {

        DefaultConfig(builder, "FATURAS", "FATURA");

        _ = builder.Property(e => e.NumeroFatura)
            .IsRequired()
            .HasColumnName($"NUMERO_FATURA")
            .HasColumnType("NUMBER(8)");

        _ = builder.OwnsOne(
            o => o.EnderecoEntrega,
            pp =>
            {
                _ = pp.Property(p => p.Cidade)
                    .HasColumnName("ENTREGA_CIDADE")
                    .HasColumnType($"VARCHAR2({Endereco.MaxCidade})");

                _ = pp.Property(p => p.Logradouro)
                    .HasColumnName("ENTREGA_LOGRADOURO")
                    .HasColumnType($"VARCHAR2({Endereco.MaxLogradouro})");

            });

        _ = builder.OwnsOne(
            o => o.EnderecoFatura,
            pp =>
            {
                _ = pp.Property(p => p.Cidade)
                    .HasColumnName("FATURA_CIDADE")
                    .HasColumnType($"VARCHAR2({Endereco.MaxCidade})");

                _ = pp.Property(p => p.Logradouro)
                    .HasColumnName("FATURA_LOGRADOURO")
                    .HasColumnType($"VARCHAR2({Endereco.MaxLogradouro})");

            });

        AuditConfig(builder);
        AuditUserIdIgnore(builder);

    }
}
