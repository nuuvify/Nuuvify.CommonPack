using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Nuuvify.CommonPack.Domain.FluentValidatorR;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Domain
{
    public partial class ValidationConcernR<T> where T : NotifiableR
    {
        private readonly T _validatable;
        public bool AssertValid { get; private set; } = false;
        public IList<NotificationR> Errors { get; private set; }


        public ValidationConcernR(T validatable)
        {
            _validatable = validatable;
            Errors = new List<NotificationR>();

        }


        /// <summary>
        /// Esse metodo remove as mensagens de Selector: nulo. Isso é util quando você personaliza suas mensagens
        /// e não deseja que a mensagem default seja lançada.
        /// <para>Esse metodo deve ser chamado sempre em ultimo lugar no ValidationConcernR</para>
        /// <para>Usage:</para>
        /// <example>
        /// <code>
        ///  new ValidationConcernR{OtherClass}(otherClass)
        ///      .AssertIsNull(otherClass)
        ///      .AssertNotIsNull(customer, messageExpected)
        ///      .AssertHasMinLength(x => customer.Name, 10)
        ///      .AssertIsGreaterOrEqualsThan(x => customer.Height, 10D)
        ///      .RemoveSelectorMessage();
        /// </code>
        /// </example>
        /// </summary>
        public void RemoveSelectorMessage()
        {
            var errorNull = Errors.Where(x => x.Message.StartsWith("Selector: "));
            foreach (var item in errorNull)
            {
                Errors.Remove(item);
            }

            if (!(_validatable is null))
            {
                var messages = _validatable.Notifications
                    .Where(n => n.Message
                    .StartsWith($"Selector: "))
                    .ToList();

                _validatable.RemoveNotifications(messages);
            }

        }

        private MethodBase MethodBaseConcern
        {
            get
            {
                StackTrace st = new StackTrace(true);
                StackFrame sf = st.GetFrame(2);
                return sf.GetMethod();
            }
            set
            {
                MethodBaseConcern = value;
            }
        }
        private bool DataBool { get; set; }
        private string DataString { get; set; }
        private long DataLong { get; set; }
        private int DataInt { get; set; }
        private int DataByte { get; set; }
        private double DataDouble { get; set; }
        private decimal DataDecimal { get; set; }
        private float DataFloat { get; set; }
        private DateTime? DataDt { get; set; }
        private DateTimeOffset? DataDtof { get; set; }
        private string NameT { get; set; }
        private string Name { get; set; }
        private string Field { get; set; }
        private string FieldA { get; set; }
        private string FieldB { get; set; }
        private string FieldMax { get; set; }
        private string FieldMin { get; set; }
        private string SelectorNull { get; set; }


        private void ResetVariables()
        {
            DataBool = default;
            DataString = default;
            DataLong = default;
            DataInt = default;
            DataByte = default;
            DataDouble = default;
            DataDecimal = default;
            DataFloat = default;
            DataDt = default;
            DataDtof = default;
            NameT = default;
            Name = default;
            Field = default;
            FieldA = default;
            FieldB = default;
            FieldMax = default;
            FieldMin = default;
            SelectorNull = default;
        }

        private void ConfigConcernMenssage(string methodName, Type TParameter, string message = "", object val = null, string aggregateId = null)
        {
            if (Errors.Count > 0) return;

            var msg = string.IsNullOrWhiteSpace(message)
                ? MsgFluentValidator.ResourceManager.GetString(methodName, CultureInfo.CurrentCulture)
                : message;

            val = val is null ? "" : val;
            Field = Field is null ? "" : Field;
            FieldA = FieldA is null ? "" : FieldA;
            FieldB = FieldB is null ? "" : FieldB;
            FieldMin = FieldMin is null ? "" : FieldMin;
            FieldMax = FieldMax is null ? "" : FieldMax;

            msg = msg.Replace("{name}", Name)
                     .Replace("{value}", Field)
                     .Replace("{field}", Field)
                     .Replace("{data}", Field)
                     .Replace("{length}", Field)
                     .Replace("{a}", FieldA)
                     .Replace("{b}", FieldB)
                     .Replace("{number}", Field)
                     .Replace("{max}", FieldMax)
                     .Replace("{min}", FieldMin)
                     .Replace("{selector}", SelectorNull);

            msg = $"{msg} {val}".Trim();

            if (_validatable is null)
            {
                ConfigConcernMessageObjectNull(TParameter, message, aggregateId);
            }
            else
            {

                var messageExist = _validatable.Notifications.FirstOrDefault(n => n.Message.StartsWith($"Selector: {SelectorNull}"));

                if (messageExist is null || !msg.Equals(messageExist?.Message))
                {
                    
                    _validatable.AddNotification(new NotificationR($"{NameT}.{Name}.{MethodBaseConcern.Name}", 
                        msg, 
                        aggregateId,
                        NameT,
                        TParameter));
                }
            }
        }

        private void ConfigConcernMessageObjectNull(Type TParameter, string msg = null, string aggregateId = null)
        {

            var errorNull = Errors.FirstOrDefault(x => x.Property.Equals(nameof(AssertNotIsNull)) || x.Property.Equals(".ctor"));
            if (errorNull != null)
            {
                Errors.Remove(errorNull);
            }
            Errors.Add(new NotificationR($"{MethodBaseConcern.Name}", msg, aggregateId, "", TParameter));
        }

    }
}
