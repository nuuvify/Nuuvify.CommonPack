using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nuuvify.CommonPack.Domain;
using Nuuvify.CommonPack.UnitOfWork.Abstraction;

namespace Nuuvify.CommonPack.UnitOfWork;

/// <summary>
/// Esta classe deve ser herdada pela sua classe de config do EntityFramework, <br/>
/// ela configura os campos de auditoria. <br/>
/// <para>
/// IMPORTANTE: O namespace da sua classe de config deve terminar sempre com o nome <br/>
/// da classe de contexto que ela faz parte, exemplo: namespace MinhaEmpresa.MinhaApp.Infra.Data.Configs.AppDbContext <br/>
///  <br/>
/// Utilize:  DefaultConfig(builder, "FATURAS", "FATURA"); <br/>
/// Auditoria:      AuditConfig(builder);  <br/>
/// se necessario:  AuditUserIdConfig(builder);  <br/>
/// ou ignore, caso sua tabela não necessite desses campos:   <br/>
/// AuditIgnore(builder);  <br/>
/// AuditUserIdIgnore(builder); <br/>
/// </para>   
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public abstract class EntityConfiguration<TEntity> :
    IEntityTypeConfiguration<TEntity> where TEntity : DomainEntity
{

    public abstract void Configure(EntityTypeBuilder<TEntity> builder);

    /// <summary>
    /// Sera mapeado a PK para o EF conforme o parametro informado, <br/>
    /// <para>
    /// esse campo na tabela será mapeado com a propriedade ID contida na classe 
    /// de dominio TEntity <br/>
    /// Caso necessite de uma PK com multiplos campos, crie no seu arquivo config:  <br/>
    /// <br/>
    /// builder.HasIndex(x => new {x.PROP1, x.PROP2, x.PROP3}) <br/>
    ///        .HasName("IX_NOME_DO_INDICE") <br/>
    ///        .IsUnique(); <br/>
    ///  <br/>
    /// Para ignorar esse metodo, e fazer esse mapeamento manual, basta passar os parametros null <br/>
    ///  <br/>
    /// DefaultConfig(builder, null, null)
    /// </para>
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="tableName">Nome da tabela no banco de dados</param>
    /// <param name="idColumnName">Nome da coluna PK</param>
    /// <param name="pkSufix">Informe Null caso não queira o sufixo no final do campo PK, ou informe o sufixo que desejar</param>
    protected virtual void DefaultConfig(EntityTypeBuilder<TEntity> builder,
        string tableName, string idColumnName, string pkSufix = "_ID")
    {

        if (string.IsNullOrWhiteSpace(tableName) || string.IsNullOrWhiteSpace(idColumnName))
        {
            return;
        }

        if (ProviderSelected.IsProviderOracle() ||
            ProviderSelected.IsProviderDb2())
        {
            _ = builder.ToTable(tableName.ToUpper());

            _ = builder.HasKey(x => x.Id)
                .HasName($"PK_{idColumnName.ToUpper()}");

            _ = builder.Property(x => x.Id)
                .HasColumnName($"{idColumnName.ToUpper()}{pkSufix}")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxId)
                .IsRequired();
        }
        else if (ProviderSelected.IsProviderSqlServer() ||
                 ProviderSelected.IsProviderSqLite())
        {

            _ = builder.ToTable(tableName);

            _ = builder.HasKey(x => x.Id)
                .HasName($"PK_{idColumnName}");

            _ = builder.Property(x => x.Id)
                .HasColumnName($"{idColumnName}{pkSufix}")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxId)
                .IsRequired();
        }
        else if (ProviderSelected.IsProviderPostgreSQL())
        {

            _ = builder.ToTable(tableName);

            _ = builder.HasKey(x => x.Id)
                .HasName($"pk_{idColumnName}");

            _ = builder.Property(x => x.Id)
                .HasColumnName($"{idColumnName}{pkSufix}")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxId)
                .IsRequired();
        }
        else
        {
            throw new ArgumentException(message: "Provider informado não é suportado", paramName: ProviderSelected.ProviderName);
        }

    }

    /// <summary>
    /// Todas as tabelas devem possuir esses campos de auditoria
    /// <para>
    /// USUARIO_INCLUSAO <br/>
    /// USUARIO_ALTERACAO <br/>
    /// DATA_INCLUSAO <br/>
    /// DATA_ALTERACAO <br/>
    /// </para>
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void AuditConfig(EntityTypeBuilder<TEntity> builder)
    {

        if (ProviderSelected.IsProviderOracle() ||
            ProviderSelected.IsProviderDb2())
        {
            _ = builder.Property(x => x.UsuarioCadastro)
                .IsRequired()
                .HasColumnName("USUARIO_INCLUSAO")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioCadastro);

            _ = builder.Property(x => x.UsuarioAlteracao)
                .HasColumnName("USUARIO_ALTERACAO")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioAlteracao);

            _ = builder.Property(x => x.DataCadastro)
                .IsRequired()
                .HasColumnName("DATA_INCLUSAO");

            _ = builder.Property(x => x.DataAlteracao)
                .HasColumnName("DATA_ALTERACAO");

        }
        else if (ProviderSelected.IsProviderSqlServer() ||
                 ProviderSelected.IsProviderSqLite())
        {
            _ = builder.Property(x => x.UsuarioCadastro)
                .IsRequired()
                .HasColumnName("USUARIO_INCLUSAO")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioCadastro);

            _ = builder.Property(x => x.UsuarioAlteracao)
                .HasColumnName("USUARIO_ALTERACAO")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioAlteracao);

            _ = builder.Property(x => x.DataCadastro)
                .IsRequired()
                .HasColumnName("DATA_INCLUSAO");

            _ = builder.Property(x => x.DataAlteracao)
                .HasColumnName("DATA_ALTERACAO");
        }
        else if (ProviderSelected.IsProviderPostgreSQL())
        {
            _ = builder.Property(x => x.UsuarioCadastro)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioCadastro);

            _ = builder.Property(x => x.UsuarioAlteracao)
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioAlteracao);

            _ = builder.Property(x => x.DataCadastro)
                .IsRequired();

            _ = builder.Property(x => x.DataAlteracao);
        }
        else
        {
            throw new ArgumentException(message: "Provider informado não é suportado", paramName: ProviderSelected.ProviderName);
        }

    }

    /// <summary>
    /// Ignore os campos de auditoria apenas para tabelas de sistemas legados, <br/>
    /// sistema novo deve possuir todos os campos de auditoria
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void AuditIgnore(EntityTypeBuilder<TEntity> builder)
    {

        _ = builder.Ignore(x => x.UsuarioCadastro);

        _ = builder.Ignore(x => x.DataCadastro);

        _ = builder.Ignore(x => x.UsuarioAlteracao);

        _ = builder.Ignore(x => x.DataAlteracao);

    }

    /// <summary>
    /// Extenção dos campos de auditoria, implementam o ID do usuario de inclusão/alteração <br/>
    /// USUARIO_ID_INCLUSAO <br/>
    /// USUARIO_ID_ALTERACAO
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void AuditUserIdConfig(EntityTypeBuilder<TEntity> builder)
    {

        if (ProviderSelected.IsProviderOracle() ||
            ProviderSelected.IsProviderDb2())
        {
            _ = builder.Property(x => x.UsuarioIdCadastro)
                .IsRequired()
                .HasColumnName("USUARIO_ID_INCLUSAO")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioIdCadastro);

            _ = builder.Property(x => x.UsuarioIdAlteracao)
                .HasColumnName("USUARIO_ID_ALTERACAO")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioIdAlteracao);
        }
        else if (ProviderSelected.IsProviderSqlServer() ||
                 ProviderSelected.IsProviderSqLite())
        {
            _ = builder.Property(x => x.UsuarioIdCadastro)
                .IsRequired()
                .HasColumnName("USUARIO_ID_INCLUSAO")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioIdCadastro);

            _ = builder.Property(x => x.UsuarioIdAlteracao)
                .HasColumnName("USUARIO_ID_ALTERACAO")
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioIdAlteracao);
        }
        else if (ProviderSelected.IsProviderPostgreSQL())
        {
            _ = builder.Property(x => x.UsuarioIdCadastro)
                .IsRequired()
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioIdCadastro);

            _ = builder.Property(x => x.UsuarioIdAlteracao)
                .IsUnicode(false)
                .HasMaxLength(DomainEntity.MaxUsuarioIdAlteracao);
        }
        else
        {
            throw new ArgumentException(message: "Provider informado não é suportado", paramName: ProviderSelected.ProviderName);
        }

    }

    /// <summary>
    /// Ignore os campos de extenção de auditoria, caso sua aplicação não necessite desses <br/>
    /// campos, contudo, os campos padrões de auditoria são obrigatorios.
    /// </summary>
    /// <param name="builder"></param>
    protected virtual void AuditUserIdIgnore(EntityTypeBuilder<TEntity> builder)
    {

        _ = builder.Ignore(x => x.UsuarioIdCadastro);

        _ = builder.Ignore(x => x.UsuarioIdAlteracao);

    }

}

