using System.Linq.Expressions;
using System.Reflection;
using Nuuvify.CommonPack.Extensions.Implementation;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Domain;


public partial class ValidationConcernR<T> where T : NotifiableR
{

    private void ConfigConcern(Expression<Func<T, bool>> selector)
    {
        ResetVariables();

        var isnull = AssertNotIsNull(_validatable);
        if (isnull.Errors.Count > 0) return;

        try
        {
            SelectorNull = null;
            DataBool = selector.Compile().Invoke(_validatable);
        }
        catch (Exception)
        {

            SelectorNull = "IsNull";
            DataBool = false;
        }

        NameT = _validatable?.GetType()?.Name;

        if (selector.Body is MemberExpression memberExpression)
        {
            var memberName = string.Empty;

            if (memberExpression.Member is null)
            {
                memberName = memberExpression.ToString();
                SelectorNull = memberName;
            }
            else
            {
                memberName = memberExpression?.Member?.Name;
                SelectorNull = SelectorNull == "IsNull" ? memberExpression?.Expression?.Type?.Name ?? memberExpression?.Type.Name : "";
            }

            Name = memberName;
        }
        else if (selector.Body is BinaryExpression binaryExpression)
        {
            if (!string.IsNullOrWhiteSpace(SelectorNull) && SelectorNull == "IsNull" &&
                binaryExpression.Left is Expression leftExpression &&
                leftExpression is MemberExpression member)
            {

                SelectorNull = member.Member?.ReflectedType?.Name;
            }

            var methodName = binaryExpression.Method is null
                ? binaryExpression.ToString()
                : binaryExpression?.Method.Name;

            Name = methodName;
        }
        else if (selector.Body is ConstantExpression contantExpression)
        {
            Name = contantExpression?.Value?.ToString();
        }
        else
        {
            Name = "";
        }
    }

    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertNotIsNull(_customer);
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="objectVerification">Object check</param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    /// <returns>Errors.Count</returns>
    public ValidationConcernR<T> AssertNotIsNull(T objectVerification, string message = "", string aggregateId = null)
    {

        if (!objectVerification.NotNull())
        {
            var classObject = typeof(T).GetTypeInfo();
            Name = classObject.Name;
            ConfigConcernMenssage(nameof(AssertNotIsNull), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }
    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertNotIsNull{OtherClass}(_otherClass);
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="objectVerification">Another class for verification, other than typed class</param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    /// <returns>Errors.Count</returns>
    public ValidationConcernR<T> AssertNotIsNull<TObject>(TObject objectVerification, string message = "", string aggregateId = null) where TObject : class
    {
        if (!objectVerification.NotNull())
        {
            var classObject = typeof(TObject).GetTypeInfo();
            Name = classObject.Name;
            ConfigConcernMenssage(nameof(AssertNotIsNull), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }
    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertIsNull(_customer);
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="objectVerification">Object check</param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    /// <returns>Errors.Count</returns>
    public ValidationConcernR<T> AssertIsNull(T objectVerification, string message = "", string aggregateId = null)
    {
        if (objectVerification.NotNull())
        {
            var classObject = typeof(T).GetTypeInfo();
            Name = classObject.Name;
            ConfigConcernMenssage(nameof(AssertIsNull), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            Errors.Clear();
            AssertValid = true;
        }
        return this;
    }

    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertIsNull{OtherClass}(_otherClass)
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="objectVerification">Another class for verification, other than typed class</param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    /// <returns>Errors.Count</returns>
    public ValidationConcernR<T> AssertIsNull<TObject>(TObject objectVerification, string message = "", string aggregateId = null) where TObject : class
    {
        if (objectVerification.NotNull())
        {
            var classObject = typeof(TObject).GetTypeInfo();
            Name = classObject.Name;
            ConfigConcernMenssage(nameof(AssertIsNull), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            Errors.Clear();
            AssertValid = true;
        }
        return this;
    }

    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertAreEquals(x => x.Qtd + 10 == 50, true);
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="selector">Lambda Expression</param>
    /// <param name="val">Expected result of verification</param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    public ValidationConcernR<T> AssertAreEquals(Expression<Func<T, bool>> selector, bool val, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (DataBool != val)
        {
            Field = val.ToString().ToLower();
            ConfigConcernMenssage(nameof(AssertAreEquals), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }
    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertAreEquals(x => x.Name, "João");
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="selector">Lambda Expression</param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    public ValidationConcernR<T> AssertAreEquals(Expression<Func<T, bool>> selector, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (!DataBool)
        {
            Field = "true";
            ConfigConcernMenssage(nameof(AssertAreEquals), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }
    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertNotAreEquals(x => x.Name == "João", false);
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="selector">Lambda Expression</param>
    /// <param name="val">Expected result of verification</param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    public ValidationConcernR<T> AssertNotAreEquals(Expression<Func<T, bool>> selector, bool val, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (DataBool == val)
        {
            Field = val.ToString().ToLower();
            ConfigConcernMenssage(nameof(AssertNotAreEquals), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }
    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertNotAreEquals(x => x.Name, "João");
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="selector">Lambda Expression</param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    public ValidationConcernR<T> AssertNotAreEquals(Expression<Func<T, bool>> selector, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (DataBool)
        {
            Field = "false";
            ConfigConcernMenssage(nameof(AssertNotAreEquals), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }
    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertIsTrue(_customer.Name.Equal("Fulano"), nameof(_customer.Name));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="value">Comparation result</param>
    /// <param name="field">Name of property (any)</param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    public ValidationConcernR<T> AssertIsTrue(bool value, string field, string message = "", string aggregateId = null)
    {
        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (!value)
        {
            Field = field;
            ConfigConcernMenssage(nameof(AssertIsTrue), typeof(T), message: message, val: value, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }

    /// <summary>
    /// Usage:
    /// <example>
    /// <code>
    ///        var valido = new ValidationConcernR{Customer}(_customer)
    ///             .AssertIsTrue(x => x.Name.Equal("Fulano"));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="selector"></param>
    /// <param name="message">Custom Message</param>
    /// <param name="aggregateId">Aggregate id or any information that allows identifying this message in a collection</param>
    public ValidationConcernR<T> AssertIsTrue(Expression<Func<T, bool>> selector, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (!DataBool)
        {
            Field = Name;
            ConfigConcernMenssage(nameof(AssertIsTrue), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }
}
