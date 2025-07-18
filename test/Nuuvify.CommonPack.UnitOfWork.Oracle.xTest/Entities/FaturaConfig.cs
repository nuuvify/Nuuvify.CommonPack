using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nuuvify.CommonPack.UnitOfWork.Oracle.xTest.Entities.StubDbContext
{
    public class FaturaConfig : EntityConfiguration<Fatura>
    {

        public override void Configure(EntityTypeBuilder<Fatura> builder)
        {

            DefaultConfig(builder, "FATURAS", "FATURA");


            builder.Property(e => e.NumeroFatura)
                .IsRequired()
                .HasColumnName($"NUMERO_FATURA")
                .HasColumnType("NUMBER(8)");


            builder.OwnsOne(
                o => o.EnderecoEntrega,
                pp =>
                {
                    pp.Property(p => p.Cidade)
                        .HasColumnName("ENTREGA_CIDADE")
                        .HasColumnType($"VARCHAR2({Endereco.MaxCidade})");

                    pp.Property(p => p.Logradouro)
                        .HasColumnName("ENTREGA_LOGRADOURO")
                        .HasColumnType($"VARCHAR2({Endereco.MaxLogradouro})");

                });

            builder.OwnsOne(
                o => o.EnderecoFatura,
                pp =>
                {
                    pp.Property(p => p.Cidade)
                        .HasColumnName("FATURA_CIDADE")
                        .HasColumnType($"VARCHAR2({Endereco.MaxCidade})");

                    pp.Property(p => p.Logradouro)
                        .HasColumnName("FATURA_LOGRADOURO")
                        .HasColumnType($"VARCHAR2({Endereco.MaxLogradouro})");

                });


            AuditConfig(builder);
            AuditUserIdIgnore(builder);


        }
    }
}
