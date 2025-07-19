using System.Collections.Generic;
using System.Linq;

namespace Nuuvify.CommonPack.Extensions.Implementation
{

    public static class ValidatedNotNullExtensionMethods
    {

        public static bool NotNull<T>(this T value) where T : class
        {
            return !(value is null);
        }

        public static bool NotNullOrZero<T>(this T value) where T : IEnumerable<object>
        {
            var isNull = value != null;

            var isZero = value?.Count() != 0;

            return isZero && isNull;
        }
        public static bool NotNullOrZero(this System.Collections.IEnumerable value)
        {
            var isNull = value == null;
            if (isNull) return false;

            var notNullOrZero = false;
            var enumerator = value.GetEnumerator();
            if (enumerator != null)
            {
                notNullOrZero = enumerator.MoveNext();
            }

            return notNullOrZero;

        }
    }

}
