using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Nuuvify.CommonPack.Extensions.Notificator;

namespace Nuuvify.CommonPack.Extensions.Implementation
{
    public static class DataAnnotationExtension
    {

        private static IList<NotificationR> _notifications;

        public static IList<NotificationR> GetNotifications(this IDataAnnotationCustom model)
        {
            return _notifications.ToList();
        }

        public static bool DataAnnotationsIsValid(this IDataAnnotationCustom model)
        {

            var context = new ValidationContext(model, null, null);

            var validationResults = new List<ValidationResult>();

            NotificationR notification;
            _notifications = new List<NotificationR>();

            if (!Validator.TryValidateObject(model, context, validationResults, true))
            {
                foreach (var validationResult in validationResults)
                {
                    notification = new NotificationR(
                        property: nameof(DataAnnotationsIsValid),
                        message: validationResult.ErrorMessage,
                        aggregatorId: null,
                        type: nameof(model),
                        model.GetType());

                    _notifications.Add(notification);
                }
            }

            return _notifications.Count == 0;
        }


    }
}