namespace Weikio.Common
{
    /// <summary>
    ///     Reflection helpers
    ///     https://stackoverflow.com/a/1954663/66988
    /// </summary>
    public static class ReflectionHelpers
    {
        public static object GetPropertyValue(this object obj, string name)
        {
            foreach (var part in name.Split('.'))
            {
                if (obj == null)
                {
                    return null;
                }

                var type = obj.GetType();
                var info = type.GetProperty(part);

                if (info == null)
                {
                    return null;
                }

                obj = info.GetValue(obj, null);
            }

            return obj;
        }

        public static T GetPropertyValue<T>(this object obj, string name, T defaultValue)
        {
            var retval = GetPropertyValue(obj, name);

            if (retval == null)
            {
                return defaultValue;
            }

            // throws InvalidCastException if types are incompatible
            return (T) retval;
        }
    }
}
