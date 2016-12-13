using System;

namespace B9PartSwitch
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object o)
        {
            return (o == null);
        }

        public static bool IsNotNull(this object o)
        {
            return (o != null);
        }

        public static void ThrowIfNullArgument(this object o, string paramName)
        {
            if (o.IsNull()) throw new ArgumentNullException(paramName);
        }
    }
}
