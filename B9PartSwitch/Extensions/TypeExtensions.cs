using System;
using UnityEngine;

namespace B9PartSwitch
{
    public static class TypeExtensions
    {
        public static bool Implements(this Type self, Type other)
        {
            return other.IsAssignableFrom(self);
        }

        public static bool Implements<T>(this Type self) => self.Implements(typeof(T));

        public static bool IsUnitySerializableType(this Type t)
        {
            if (t == null)
                return false;
            if (t.IsPrimitive) return true;
            if (t == typeof(string)) return true;
            if (t.IsSubclassOf(typeof(UnityEngine.Object))) return true;
            if (t.IsListType() && t.GetGenericArguments()[0].IsUnitySerializableType()) return true;
            if (t.IsArray && t.GetElementType().IsUnitySerializableType()) return true;
            if (t.IsEnum) return true;
            if (t.IsSerializable) return true;

            // Unity serializable types
            if (t == typeof(Vector2)) return true;
            if (t == typeof(Vector3)) return true;
            if (t == typeof(Vector4)) return true;
            if (t == typeof(Quaternion)) return true;
            if (t == typeof(Matrix4x4)) return true;
            if (t == typeof(Color)) return true;
            if (t == typeof(Rect)) return true;
            if (t == typeof(LayerMask)) return true;

            return false;
        }
    }
}
