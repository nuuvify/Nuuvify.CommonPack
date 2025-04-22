using System.Linq.Expressions;
using Nuuvify.CommonPack.Mediator.Implementation;

namespace Nuuvify.CommonPack.Domain;

public partial class ValidationConcernR<T> where T : NotifiableR
{

    private void ConfigConcern(Expression<Func<T, string>> selector)
    {
        ResetVariables();

        var isnull = AssertNotIsNull(_validatable);
        if (isnull.Errors.Count > 0) return;

        try
        {
            SelectorNull = null;
            DataString = selector.Compile().Invoke(_validatable);
        }
        catch (Exception)
        {
            SelectorNull = "IsNull";
            DataString = default;
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

    public ValidationConcernR<T> AssertIsRequired(Expression<Func<T, string>> selector, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (string.IsNullOrWhiteSpace(DataString))
        {
            ConfigConcernMenssage(nameof(AssertIsRequired), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertIsNullOrWhiteSpace(Expression<Func<T, string>> selector, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(DataString))
        {
            ConfigConcernMenssage(nameof(AssertIsNullOrWhiteSpace), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }

        return this;
    }

    public ValidationConcernR<T> AssertNotIsNullOrWhiteSpace(Expression<Func<T, string>> selector, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (string.IsNullOrWhiteSpace(DataString))
        {
            ConfigConcernMenssage(nameof(AssertNotIsNullOrWhiteSpace), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }

    public ValidationConcernR<T> AssertContains(Expression<Func<T, string>> selector, string text, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (DataString == null || !DataString.Any(text.Contains))
        {
            Field = text;
            ConfigConcernMenssage(nameof(AssertContains), typeof(T), message: message, val: DataString, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }

    public ValidationConcernR<T> AssertContains(Expression<Func<T, string>> selector, string[] text, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        bool encontrou = false;
        if (text is null)
        {
            ConfigConcernMenssage("AssertContainsArrayNull", typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            encontrou = Array.Exists<string>(text, delegate (string s)
            {
                if (DataString == null) return false;
                return DataString.IndexOf(s, StringComparison.OrdinalIgnoreCase) > -1;
            });
        }
        if (!encontrou)
        {
            ConfigConcernMenssage("AssertContainsArray", typeof(T), message: message, aggregateId: aggregateId);
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

    public ValidationConcernR<T> AssertAreEquals(Expression<Func<T, string>> selector, string text, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (string.IsNullOrWhiteSpace(DataString) || !DataString.Equals(text, StringComparison.OrdinalIgnoreCase))
        {
            Field = text;
            ConfigConcernMenssage(nameof(AssertAreEquals), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }

    public ValidationConcernR<T> AssertNotAreEquals(Expression<Func<T, string>> selector, string text, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(DataString) && DataString.Equals(text, StringComparison.OrdinalIgnoreCase))
        {
            Field = text;
            ConfigConcernMenssage(nameof(AssertNotAreEquals), typeof(T), message: message, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }

    public ValidationConcernR<T> AssertFixedLength(Expression<Func<T, string>> selector, int length, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (string.IsNullOrWhiteSpace(DataString) || DataString.Trim().Length != length)
        {
            Field = length.ToString();
            ConfigConcernMenssage(nameof(AssertFixedLength), typeof(T), message: message, val: DataString, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }

    public ValidationConcernR<T> AssertHasMinLength(Expression<Func<T, string>> selector, int min, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (string.IsNullOrWhiteSpace(DataString) || DataString.Trim().Length < min)
        {
            FieldMin = min.ToString();
            ConfigConcernMenssage(nameof(AssertHasMinLength), typeof(T), message: message, val: DataString, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }

    public ValidationConcernR<T> AssertHasMaxLength(Expression<Func<T, string>> selector, int max, string message = "", string aggregateId = null)
    {
        ConfigConcern(selector);

        if (!string.IsNullOrWhiteSpace(SelectorNull))
        {
            ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
        }
        else if (!string.IsNullOrWhiteSpace(DataString) && DataString.Trim().Length > max)
        {
            FieldMax = max.ToString();
            ConfigConcernMenssage(nameof(AssertHasMaxLength), typeof(T), message: message, val: DataString, aggregateId: aggregateId);
        }
        else
        {
            AssertValid = true;
        }
        return this;
    }

}
