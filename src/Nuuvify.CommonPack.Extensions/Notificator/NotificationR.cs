using System;
using Microsoft.Extensions.Logging;
using Nuuvify.CommonPack.Extensions.Interfaces;

namespace Nuuvify.CommonPack.Extensions.Notificator
{

    public class NotificationR : INotPersistingAsTable
    {

        protected NotificationR() { }
        public NotificationR(string property, string message)
        {
            DefineProperty(property);
            DefineMessage(message);
            DefineDateOccurrence(DateTimeOffset.Now);

        }
        public NotificationR(string property, string message, string aggregatorId)
        {
            DefineProperty(property);
            DefineMessage(message);
            DefineAggregatorId(aggregatorId);
            DefineDateOccurrence(DateTimeOffset.Now);
        }
        public NotificationR(string property, string message, string aggregatorId, string type, Type originNotification)
        {
            DefineProperty(property);
            DefineMessage(message);
            DefineAggregatorId(aggregatorId);
            DefineType(type);
            DefineOriginNotification(originNotification);
            DefineDateOccurrence(DateTimeOffset.Now);
        }


        private void DefineProperty(string property)
        {
            Property = property;

        }
        private void DefineMessage(string message)
        {
            Message = message;

        }
        private void DefineType(string type)
        {
            Type = type;

        }
        private void DefineAggregatorId(string aggregatorId)
        {
            AggregatorId = aggregatorId;

        }
        private void DefineDateOccurrence(DateTimeOffset dateOccurrence)
        {
            DateOccurrence = dateOccurrence;

        }

        private void DefineOriginNotification(Type originNotification)
        {
            if (originNotification != null)
            {
                OriginNotification = originNotification?.Name;
            }
        }



        /// <summary>
        /// Alem de retornar as propriedades, também é adicionado ao seu mecanismo de log
        /// as mesmas propriedades, caso não queira logar, basta informar Null no logger.
        /// A classe sera retornada mesmo sem log
        /// </summary>
        /// <param name="logger">Mecanismo de log configurado na aplicação</param>
        /// <param name="logDescription">Normalmente utilizado para que vc possa identificar o registro no arquivo de log</param>
        /// <param name="logLevel">Tipo de log</param>
        /// <returns></returns>
        public NotificationR LoggerNotification(ILogger logger, string logDescription, LogLevel logLevel = LogLevel.Warning)
        {
            if (!(logger is null))
            {

                logger.Log(logLevel, "Validação em: {0} {1} {2} {3} {4} {5}",
                    logDescription,
                    Property,
                    Message,
                    Type,
                    AggregatorId,
                    DateOccurrence);

            }

            return this;

        }


        public string AggregatorId { get; private set; }
        public string Property { get; private set; }
        public string Message { get; private set; }
        public string Type { get; private set; }
        public string OriginNotification { get; private set; }
        public DateTimeOffset DateOccurrence { get; private set; }


    }
}