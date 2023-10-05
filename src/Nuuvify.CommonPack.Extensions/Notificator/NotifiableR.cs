using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace Nuuvify.CommonPack.Extensions.Notificator
{
    public abstract class NotifiableR
    {

        private readonly List<NotificationR> _notifications;

        protected NotifiableR()
        {
            _notifications = new List<NotificationR>();
        }




        [JsonIgnore]
        public virtual IReadOnlyCollection<NotificationR> Notifications => _notifications;

        /// <summary>
        /// Alem de retornar a lista de notificações, também é adicionado ao seu mecanismo de log
        /// as mesmas informações, caso não queira logar, basta informar Null no logger.
        /// A lista sera retornada mesmo sem log
        /// </summary>
        /// <param name="logger">Mecanismo de log configurado na aplicação. Sera utilizado LogWarning</param>
        /// <param name="logDescription">Normalmente utilizado para que vc possa identificar o registro no arquivo de log</param>
        /// <returns></returns>
        public IList<NotificationR> LoggerNotifications(ILogger logger, string logDescription)
        {
            if (!(logger is null))
            {

                _notifications.ForEach(delegate (NotificationR notification)
                {
                    logger.LogWarning("Validação em: {0} {1} {2} {3} {4} {5}",
                            logDescription,
                            notification.Property,
                            notification.Message,
                            notification.Type,
                            notification.AggregatorId,
                            notification.DateOccurrence);
                });
            }

            return _notifications;

        }


        public void RemoveNotification(string property)
        {
            if (!(property is null))
                _notifications.RemoveAll(x => x.Property.Equals(property));

        }
        public void RemoveNotifications(IList<NotificationR> notifications)
        {
            notifications?.ToList().ForEach(delegate (NotificationR notificationR)
            {
                _notifications.Remove(notificationR);
            });


        }
        public void RemoveNotifications(bool removeAll = true)
        {
            if (removeAll && !(_notifications is null))
            {
                _notifications.RemoveAll(x => x.Property != null);

            }

        }

        public void AddNotification(string property, string message, string aggregateId = null)
        {
            _notifications.Add(new NotificationR(property, message, aggregateId));
        }

        public void AddNotification(NotificationR notification)
        {
            if (notification == null) return;
            _notifications.Add(notification);
        }

        public void AddNotifications(IReadOnlyCollection<NotificationR> notifications)
        {
            if (notifications == null) return;
            _notifications.AddRange(notifications);
        }

        public void AddNotifications(IList<NotificationR> notifications)
        {
            if (notifications?.Count() > 0)
                _notifications.AddRange(notifications);
        }

        public void AddNotifications(ICollection<NotificationR> notifications)
        {
            if (notifications?.Count() > 0)
                _notifications.AddRange(notifications);
        }

        public bool IsValid() => _notifications.Count == 0;
    }
}
