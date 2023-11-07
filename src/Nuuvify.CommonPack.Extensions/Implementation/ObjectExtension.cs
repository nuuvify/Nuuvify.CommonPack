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

    }
}
