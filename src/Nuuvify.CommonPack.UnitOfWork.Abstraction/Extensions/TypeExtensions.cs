using System.Reflection;
using Nuuvify.CommonPack.UnitOfWork.Abstraction.Constants;

namespace Nuuvify.CommonPack.UnitOfWork.Abstraction.Extensions;

/// <summary>
/// Public extension methods for Type operations.
/// </summary>
public static partial class TypeExtensions
{
    /// <summary>
    /// Determines if the property represents a collection.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <returns>True if the property is a collection; otherwise, false.</returns>
    public static bool IsPropertyACollection(this PropertyInfo property)
    {
        return IsGenericList(property.PropertyType) || property.PropertyType.IsArray;
    }

    /// <summary>
    /// Determines if the property value is an object type.
    /// </summary>
    /// <param name="property">The property to check.</param>
    /// <param name="value">The value to examine.</param>
    /// <returns>True if the property value is an object type; otherwise, false.</returns>
    public static bool IsPropertyObject(this PropertyInfo property, object value)
    {
        return Convert.GetTypeCode(property.GetValue(value, null)) == TypeCode.Object;
    }

    /// <summary>
    /// Checks if the type has a property with the specified name.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <returns>True if the property exists; otherwise, false.</returns>
    public static bool HasProperty(this Type type, string propertyName)
    {
        return type.GetAllProperties().Any(a => a.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Splits a comma-separated fields string into an array.
    /// </summary>
    /// <param name="fields">The fields string to split.</param>
    /// <returns>An array of field names.</returns>
    public static string[] Fields(this string fields)
    {
        return fields.Split(',');
    }

    /// <summary>
    /// Extracts the field name by removing sorting prefixes.
    /// </summary>
    /// <param name="field">The field string.</param>
    /// <returns>The clean field name.</returns>
    public static string FieldName(this string field)
    {
        var trimmedField = field.Trim();
        return trimmedField.StartsWith(SortConstants.DescendingPrefix) ||
            trimmedField.StartsWith(SortConstants.AscendingPrefix)
            ? trimmedField[2..]
            : trimmedField;
    }

    /// <summary>
    /// Checks if the text starts with any of the specified strings.
    /// </summary>
    /// <param name="text">The text to check.</param>
    /// <param name="with">The strings to check against.</param>
    /// <returns>True if the text starts with any of the specified strings; otherwise, false.</returns>
    public static bool StartsWith(this string text, params string[] with)
    {
        return with.Any(text.StartsWith);
    }

    /// <summary>
    /// Determines if the field indicates descending sort order.
    /// </summary>
    /// <param name="field">The field string to check.</param>
    /// <returns>True if the field indicates descending order; otherwise, false.</returns>
    public static bool IsDescending(this string field)
    {
        return field.StartsWith(SortConstants.DescendingPrefix);
    }
}
