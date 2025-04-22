using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Nuuvify.CommonPack.Extensions.Implementation;

namespace Nuuvify.CommonPack.UnitOfWork;

public static partial class ModelBuilderExtensions
{

    /// <summary>
    /// Essa extenção mapeia as propriedades esquecidas pelo desenvolvedor, atribuindo
    /// a elas tamanho 50 ao invés de serem mapeadas como max
    /// Use o metodo abaixo para informar o ProviderName desse contexto
    /// <para> 
    /// <c>
    /// protected override void OnModelCreating(ModelBuilder modelBuilder)
    /// {
    ///     modelBuilder.SetDatabaseProviderName(Database);
    /// }
    /// </c>
    ///</para>
    /// </summary>
    /// <param name="modelBuilder"></param>
    public static void MappingPropertiesForgotten(this ModelBuilder modelBuilder)
    {
        ProviderNameException(modelBuilder.GetDatabaseProviderName());

        var entities = modelBuilder.Model.GetEntityTypes()
            .Where(e => !(typeof(INotPersistingAsTable).IsAssignableFrom(e.ClrType) && !e.IsOwned()))
            .ToList();

        if (entities.NotNullOrZero())
        {
            IEnumerable<IMutableProperty> properties;

            foreach (var entity in entities)
            {
                properties = entity.GetProperties()
                    .Where(e => e.ClrType == typeof(string))
                    .ToList();

                SetColumnTypeForgotten(properties, entity.Name);
            }
        }

    }

    private static void SetColumnTypeForgotten(IEnumerable<IMutableProperty> properties, string entity)
    {

        foreach (var property in properties)
        {
            if (string.IsNullOrWhiteSpace(property.GetColumnType()) &&
                !property.GetMaxLength().HasValue)
            {
                Debug.WriteLine($"Entity: {entity} property: {property.Name}");

                property.SetMaxLength(50);
                property.SetIsUnicode(false);

            }
        }

    }

}

