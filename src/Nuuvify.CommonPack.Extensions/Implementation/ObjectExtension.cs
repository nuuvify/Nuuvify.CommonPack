using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Nuuvify.CommonPack.Extensions.Notificator;


namespace Nuuvify.CommonPack.Extensions.Implementation
{
    public static class ObjectExtension
    {
        public static bool EqualsObjectNotNull(this object obj, object obj1)
        {
            if (ValidatedNotNullExtensionMethods.NotNull<object>(obj) &&
                ValidatedNotNullExtensionMethods.NotNull<object>(obj1))
            {
                return obj.Equals(obj1);
            }

            return false;
        }



        private static IList<NotificationR> _notifications;


        public static bool DataAnnotationsIsValid(this object model)
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

        public static List<NotificationR> GetNotifications()
        {
            return _notifications.ToList();

        }

    }
}
