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

        public static void EnsureArgumentType(this object o, Type type, string paramName)
        {
            if (o.IsNotNull() && !o.GetType().DerivesFrom(type)) throw new ArgumentException($"Expected parameter of type {type} but got {o.GetType()}", paramName);
        }

        public static void EnsureArgumentType<T>(this object o, string paramName) => o.EnsureArgumentType(typeof(T), paramName);
    }
}
