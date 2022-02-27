using System;
using System.Collections.Generic;
using System.Linq;
using Nuuvify.CommonPack.Extensions.Interfaces;
using Nuuvify.CommonPack.UnitOfWork.Abstraction;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Nuuvify.CommonPack.UnitOfWork
{
    public static partial class ModelBuilderExtensions
    {

        private static void ProviderNameException(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException(message: "Nome do provider não pode ser NULL", paramName: providerName);
        }

        public static string GetDatabaseProviderName(this ModelBuilder modelBuilder)
        {
            return ProviderSelected.ProviderName;
        }

        public static void SetDatabaseProviderName(this ModelBuilder modelBuilder,
            Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade database)
        {
            ProviderNameException(database.ProviderName);
            ProviderSelected.ProviderName = database.ProviderName;
        }

        /// <summary>
        /// Extensão para ignorar objectos de valor "ValueObjects" dinamicamente
        /// Todas as classes que implementarem a interface INotPersistingAsTable
        /// serão ignoradas.
        /// </summary>
        /// <param name="modelBuilder"></param>
        /// <param name="classIgnore">Informe uma classe ou interface que será ignorada</param>
        /// <param name="isOwned">Use EIsOwner.None quando usar .UseSnakeCaseNamingConvention() </param>
        public static void IgnoreValueObject(this ModelBuilder modelBuilder, Type classIgnore = null, EIsOwner isOwned = EIsOwner.False)
        {
            IEnumerable<IMutableEntityType> entityTypes;


            if (classIgnore is null)
            {

                switch (isOwned)
                {
                    case EIsOwner.False:
                        {
                            entityTypes = modelBuilder.Model.GetEntityTypes()?
                                .Where(e => typeof(INotPersistingAsTable)
                                    .IsAssignableFrom(e.ClrType) && !e.IsOwned())?
                                .ToList();
                            break;
                        }
                    case EIsOwner.True:
                        {
                            entityTypes = modelBuilder.Model.GetEntityTypes()?
                                .Where(e => typeof(INotPersistingAsTable)
                                    .IsAssignableFrom(e.ClrType) && e.IsOwned())?
                                .ToList();
                            break;
                        }
                    default:
                        {
                            entityTypes = modelBuilder.Model.GetEntityTypes()?
                                .Where(e => typeof(INotPersistingAsTable)
                                    .IsAssignableFrom(e.ClrType))?
                                .ToList();
                            break;
                        }

                }

            }
            else
            {

                switch (isOwned)
                {
                    case EIsOwner.False:
                        {
                            entityTypes = modelBuilder.Model.GetEntityTypes()?
                                .Where(e =>
                                (
                                    (typeof(INotPersistingAsTable)
                                    .IsAssignableFrom(e.ClrType) && !e.IsOwned()) ||
                                    (classIgnore
                                    .IsAssignableFrom(e.ClrType) && !e.IsOwned())
                                ))?
                                .ToList();
                            break;
                        }
                    case EIsOwner.True:
                        {
                            entityTypes = modelBuilder.Model.GetEntityTypes()?
                                .Where(e =>
                                (
                                    (typeof(INotPersistingAsTable)
                                    .IsAssignableFrom(e.ClrType) && e.IsOwned()) ||
                                    (classIgnore
                                    .IsAssignableFrom(e.ClrType) && e.IsOwned())
                                ))?
                                .ToList();
                            break;
                        }
                    default:
                        {
                            entityTypes = modelBuilder.Model.GetEntityTypes()?
                                .Where(e =>
                                (
                                    (typeof(INotPersistingAsTable)
                                    .IsAssignableFrom(e.ClrType)) ||
                                    (classIgnore
                                    .IsAssignableFrom(e.ClrType))
                                ))?
                                .ToList();
                            break;
                        }
                }
            }



            if (entityTypes != null)
            {
                foreach (var entityType in entityTypes)
                {
                    if (entityType.ClrType.BaseType.Name == entityType.ClrType.Name ||
                        entityType.ClrType.BaseType.Name == "Object")
                        modelBuilder.Ignore(entityType.ClrType);
                }
            }

        }


    }

    public enum EIsOwner
    {
        None = 0,
        True = 1,
        False = 2
    }
}
