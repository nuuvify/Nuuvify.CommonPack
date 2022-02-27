using System;
using Nuuvify.CommonPack.Domain;
using Nuuvify.CommonPack.UnitOfWork.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Nuuvify.CommonPack.UnitOfWork
{
    /// <summary>
    /// Esta classe deve ser herdada pela sua classe de config do EntityFramework,
    /// ela configura os campos de auditoria.
    /// <para>
    /// IMPORTANTE: O namespace da sua classe de config deve terminar sempre com o nome <br/>
    /// da classe de contexto que ela faz parte, exemplo: namespace .Penf.Infra.Data.Configs.AppDbContext <br/>
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
        /// Sera mapeado a PK para o EF conforme o parametro informado, 
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



            if (ProviderSelected.IsProviderOracle())
            {
                builder.ToTable(tableName.ToUpper());

                builder.HasKey(x => x.Id)
                    .HasName($"PK_{idColumnName.ToUpper()}");

                builder.Property(x => x.Id)
                    .HasColumnName($"{idColumnName.ToUpper()}{pkSufix}")
                    .HasColumnType($"VARCHAR2({DomainEntity.MaxId})")
                    .IsRequired();
            }
            else if (ProviderSelected.IsProviderDb2())
            {
                builder.ToTable(tableName.ToUpper());

                builder.HasKey(x => x.Id)
                    .HasName($"PK_{idColumnName.ToUpper()}");

                builder.Property(x => x.Id)
                    .HasColumnName($"{idColumnName.ToUpper()}{pkSufix}")
                    .HasColumnType($"CHAR({DomainEntity.MaxId})")
                    .IsRequired();
            }
            else if (ProviderSelected.IsProviderSqlServer() ||
                     ProviderSelected.IsProviderSqLite())
            {

                builder.ToTable(tableName);

                builder.HasKey(x => x.Id)
                    .HasName($"PK_{idColumnName}");

                builder.Property(x => x.Id)
                    .HasColumnName($"{idColumnName}{pkSufix}")
                    .HasColumnType($"VARCHAR({DomainEntity.MaxId})")
                    .IsRequired();
            }
            else if (ProviderSelected.IsProviderPostgreSQL())
            {

                builder.ToTable(tableName);

                builder.HasKey(x => x.Id)
                    .HasName($"pk_{idColumnName}");

                builder.Property(x => x.Id)
                    .HasColumnName($"{idColumnName}{pkSufix}")
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

            if (ProviderSelected.IsProviderOracle())
            {
                builder.Property(x => x.UsuarioCadastro)
                    .IsRequired()
                    .HasColumnName("USUARIO_INCLUSAO")
                    .HasColumnType($"VARCHAR2({DomainEntity.MaxUsuarioCadastro})");

                builder.Property(x => x.UsuarioAlteracao)
                    .HasColumnName("USUARIO_ALTERACAO")
                    .HasColumnType($"VARCHAR2({DomainEntity.MaxUsuarioAlteracao})");

                builder.Property(x => x.DataCadastro)
                    .IsRequired()
                    .HasColumnName("DATA_INCLUSAO");

                builder.Property(x => x.DataAlteracao)
                    .HasColumnName("DATA_ALTERACAO");

            }
            else if (ProviderSelected.IsProviderDb2())
            {
                builder.Property(x => x.UsuarioCadastro)
                    .IsRequired()
                    .HasColumnName("USUARIO_INCLUSAO")
                    .HasColumnType($"CHAR({DomainEntity.MaxUsuarioCadastro})");

                builder.Property(x => x.UsuarioAlteracao)
                    .HasColumnName("USUARIO_ALTERACAO")
                    .HasColumnType($"CHAR({DomainEntity.MaxUsuarioAlteracao})");

                builder.Property(x => x.DataCadastro)
                    .IsRequired()
                    .HasColumnName("DATA_INCLUSAO");

                builder.Property(x => x.DataAlteracao)
                    .HasColumnName("DATA_ALTERACAO");

            }
            else if (ProviderSelected.IsProviderSqlServer() ||
                     ProviderSelected.IsProviderSqLite())
            {
                builder.Property(x => x.UsuarioCadastro)
                    .IsRequired()
                    .HasColumnName("USUARIO_INCLUSAO")
                    .HasColumnType($"VARCHAR({DomainEntity.MaxUsuarioCadastro})");

                builder.Property(x => x.UsuarioAlteracao)
                    .HasColumnName("USUARIO_ALTERACAO")
                    .HasColumnType($"VARCHAR({DomainEntity.MaxUsuarioAlteracao})");

                builder.Property(x => x.DataCadastro)
                    .IsRequired()
                    .HasColumnName("DATA_INCLUSAO");

                builder.Property(x => x.DataAlteracao)
                    .HasColumnName("DATA_ALTERACAO");
            }
            else if (ProviderSelected.IsProviderPostgreSQL())
            {
                builder.Property(x => x.UsuarioCadastro)
                    .IsRequired()
                    .HasMaxLength(DomainEntity.MaxUsuarioCadastro);

                builder.Property(x => x.UsuarioAlteracao)
                    .HasMaxLength(DomainEntity.MaxUsuarioAlteracao);

                builder.Property(x => x.DataCadastro)
                    .IsRequired();

                builder.Property(x => x.DataAlteracao);
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

            builder.Ignore(x => x.UsuarioCadastro);

            builder.Ignore(x => x.DataCadastro);

            builder.Ignore(x => x.UsuarioAlteracao);

            builder.Ignore(x => x.DataAlteracao);

        }


        /// <summary>
        /// Extenção dos campos de auditoria, implementam o ID do usuario de inclusão/alteração <br/>
        /// USUARIO_ID_INCLUSAO <br/>
        /// USUARIO_ID_ALTERACAO
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void AuditUserIdConfig(EntityTypeBuilder<TEntity> builder)
        {

            if (ProviderSelected.IsProviderOracle())
            {
                builder.Property(x => x.UsuarioIdCadastro)
                    .IsRequired()
                    .HasColumnName("USUARIO_ID_INCLUSAO")
                    .HasColumnType($"VARCHAR2({DomainEntity.MaxUsuarioIdCadastro})");

                builder.Property(x => x.UsuarioIdAlteracao)
                    .HasColumnName("USUARIO_ID_ALTERACAO")
                    .HasColumnType($"VARCHAR2({DomainEntity.MaxUsuarioIdAlteracao})");
            }
            else if (ProviderSelected.IsProviderDb2())
            {
                builder.Property(x => x.UsuarioIdCadastro)
                    .IsRequired()
                    .HasColumnName("USUARIO_ID_INCLUSAO")
                    .HasColumnType($"CHAR({DomainEntity.MaxUsuarioIdCadastro})");

                builder.Property(x => x.UsuarioIdAlteracao)
                    .HasColumnName("USUARIO_ID_ALTERACAO")
                    .HasColumnType($"CHAR({DomainEntity.MaxUsuarioIdAlteracao})");
            }
            else if (ProviderSelected.IsProviderSqlServer() ||
                     ProviderSelected.IsProviderSqLite())
            {
                builder.Property(x => x.UsuarioIdCadastro)
                    .IsRequired()
                    .HasColumnName("USUARIO_ID_INCLUSAO")
                    .HasColumnType($"VARCHAR({DomainEntity.MaxUsuarioIdCadastro})");

                builder.Property(x => x.UsuarioIdAlteracao)
                    .HasColumnName("USUARIO_ID_ALTERACAO")
                    .HasColumnType($"VARCHAR({DomainEntity.MaxUsuarioIdAlteracao})");
            }
            else if (ProviderSelected.IsProviderPostgreSQL())
            {
                builder.Property(x => x.UsuarioIdCadastro)
                    .IsRequired()
                    .HasMaxLength(DomainEntity.MaxUsuarioIdCadastro);

                builder.Property(x => x.UsuarioIdAlteracao)
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

            builder.Ignore(x => x.UsuarioIdCadastro);

            builder.Ignore(x => x.UsuarioIdAlteracao);

        }

    }
}
