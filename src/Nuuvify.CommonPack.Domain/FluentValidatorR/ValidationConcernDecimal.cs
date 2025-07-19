using System;
using System.Linq.Expressions;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain
{
    public partial class ValidationConcernR<T> where T : NotifiableR
    {

        private void ConfigConcern(Expression<Func<T, decimal>> selector)
        {
            ResetVariables();

            var isnull = AssertNotIsNull(_validatable);
            if (isnull.Errors.Count > 0) return;

            try
            {
                SelectorNull = null;
                DataDecimal = selector.Compile().Invoke(_validatable);
            }
            catch (Exception)
            {
                SelectorNull = "IsNull";
                DataDecimal = default;
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


        public ValidationConcernR<T> AssertAreEquals(Expression<Func<T, decimal>> selector, decimal val, string message = "", string aggregateId = null)
        {
            ConfigConcern(selector);


            if (!string.IsNullOrWhiteSpace(SelectorNull))
            {
                ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
            }
            else if(DataDecimal != val)
            {
                Field = val.ToString();
                ConfigConcernMenssage(nameof(AssertAreEquals), typeof(T), message: message, aggregateId: aggregateId);
            }
            else 
            {
                AssertValid = true;
            }

            return this;
        }
        public ValidationConcernR<T> AssertIsBetween(Expression<Func<T, decimal>> selector, decimal a, decimal b, string message = "", string aggregateId = null)
        {

            ConfigConcern(selector);


            if (!string.IsNullOrWhiteSpace(SelectorNull))
            {
                ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
            }
            else if(DataDecimal < a || DataDecimal > b)
            {
                FieldA = a.ToString();
                FieldB = b.ToString();

                ConfigConcernMenssage(nameof(AssertIsBetween), typeof(T), message: message, aggregateId: aggregateId);
            }
            else 
            {
                AssertValid = true;
            }
            return this;
        }


        public ValidationConcernR<T> AssertIsGreaterOrEqualsThan(Expression<Func<T, decimal>> selector, decimal number, string message = "", string aggregateId = null)
        {

            ConfigConcern(selector);

            if (!string.IsNullOrWhiteSpace(SelectorNull))
            {
                ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
            }
            else if(DataDecimal != number && DataDecimal < number)
            {
                Field = number.ToString();
                ConfigConcernMenssage(nameof(AssertIsGreaterOrEqualsThan), typeof(T), message: message, aggregateId: aggregateId);
            }
            else 
            {
                AssertValid = true;
            }

            return this;
        }
        public ValidationConcernR<T> AssertIsGreaterThan(Expression<Func<T, decimal>> selector, decimal number, string message = "", string aggregateId = null)
        {
            ConfigConcern(selector);

            if (!string.IsNullOrWhiteSpace(SelectorNull))
            {
                ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
            }
            else if(DataDecimal <= number)
            {
                Field = number.ToString();
                ConfigConcernMenssage(nameof(AssertIsGreaterThan), typeof(T), message: message, aggregateId: aggregateId);
            }
            else 
            {
                AssertValid = true;
            }
            return this;
        }
        public ValidationConcernR<T> AssertIsLowerOrEqualsThan(Expression<Func<T, decimal>> selector, decimal number, string message = "", string aggregateId = null)
        {

            ConfigConcern(selector);

            if (!string.IsNullOrWhiteSpace(SelectorNull))
            {
                ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
            }
            else if(DataDecimal != number && DataDecimal > number)
            {
                Field = number.ToString();
                ConfigConcernMenssage(nameof(AssertIsLowerOrEqualsThan), typeof(T), message: message, aggregateId: aggregateId);
            }
            else 
            {
                AssertValid = true;
            }

            return this;
        }
        public ValidationConcernR<T> AssertNotAreEquals(Expression<Func<T, decimal>> selector, decimal val, string message = "", string aggregateId = null)
        {

            ConfigConcern(selector);

            if (!string.IsNullOrWhiteSpace(SelectorNull))
            {
                ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
            }
            else if(DataDecimal == val)
            {
                Field = val.ToString();
                ConfigConcernMenssage(nameof(AssertNotAreEquals), typeof(T), message: message, aggregateId: aggregateId);
            }
            else 
            {
                AssertValid = true;
            }

            return this;
        }

        public ValidationConcernR<T> AssertIsLowerThan(Expression<Func<T, decimal>> selector, decimal number, string message = "", string aggregateId = null)
        {

            ConfigConcern(selector);

            if (!string.IsNullOrWhiteSpace(SelectorNull))
            {
                ConfigConcernMenssage("SelectorNull", typeof(T), aggregateId: aggregateId);
            }
            else if(DataDecimal >= number)
            {
                Field = number.ToString();
                ConfigConcernMenssage(nameof(AssertIsLowerThan), typeof(T), message: message, aggregateId: aggregateId);
            }
            else 
            {
                AssertValid = true;
            }

            return this;
        }

    }
}
