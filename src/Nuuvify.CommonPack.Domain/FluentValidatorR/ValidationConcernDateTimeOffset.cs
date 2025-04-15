using System.Linq.Expressions;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain;

public partial class ValidationConcernR<T> where T : NotifiableR
{
    private void ConfigConcern(Expression<Func<T, DateTimeOffset>> selector)
    {
        ResetVariables();

        var isnull = AssertNotIsNull(_validatable);
        if (isnull.Errors.Count > 0) return;

        try
        {
            SelectorNull = null;
            DataDtof = selector.Compile().Invoke(_validatable);
        }
        catch (Exception)
        {
            SelectorNull = "IsNull";
            DataDtof = null;
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
    public ValidationConcernR<T> AssertAreEquals(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset val, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (DataDtof != val)
        {
            Field = val.ToString();
            ConfigConcernMenssage(nameof(AssertAreEquals), typeof(T), message: message, aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertDateTimeOffsetNull(Expression<Func<T, DateTimeOffset>> selector, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!DateTimeOffset.MinValue.Equals(DataDtof) && DataDtof != null)
        {
            Field = DataDtof.ToString();
            ConfigConcernMenssage(nameof(AssertDateTimeNull), typeof(T), message: message, aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertNotDateTimeOffsetNull(Expression<Func<T, DateTimeOffset>> selector, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (DateTimeOffset.MinValue.Equals(DataDtof) || DataDtof == null)
        {
            Field = DataDtof.ToString();
            ConfigConcernMenssage(nameof(AssertDateTimeNull), typeof(T), message: message, aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertIsBetween(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset a, DateTimeOffset b, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (DataDtof < a || DataDtof > b)
        {
            FieldA = a.ToString();
            FieldB = b.ToString();

            ConfigConcernMenssage(nameof(AssertIsBetween), typeof(T), message: message, aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertIsGreaterOrEqualsThan(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (DataDtof != date && DataDtof < date)
        {
            Field = date.ToString();
            ConfigConcernMenssage(nameof(AssertIsGreaterOrEqualsThan), typeof(T), message: message, aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertIsGreaterThan(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (DataDtof <= date)
        {
            Field = date.ToString();
            ConfigConcernMenssage(nameof(AssertIsGreaterThan), typeof(T), message: message, aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertIsLowerOrEqualsThan(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (DataDt != date && DataDt > date)
        {
            Field = date.ToString();
            ConfigConcernMenssage(nameof(AssertIsLowerOrEqualsThan), typeof(T), message: message, aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertNotAreEquals(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset val, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (DataDtof == val)
        {
            Field = val.ToString();
            ConfigConcernMenssage(nameof(AssertNotAreEquals), typeof(T), message: message, aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertIsLowerThan(Expression<Func<T, DateTimeOffset>> selector, DateTimeOffset date, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (DataDtof >= date)
        {
            Field = date.ToString();
            ConfigConcernMenssage(nameof(AssertIsLowerThan), typeof(T), message: message, aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

}
