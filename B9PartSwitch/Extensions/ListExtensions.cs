using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace B9PartSwitch
{
    public static class ListExtensions
    {
        public static bool IsList(this object o)
        {
            if (o == null)
                return false;
            return IsListType(o.GetType());
        }

        public static bool IsListType(this Type t)
        {
            if (t == null)
                return false;
            return t.GetInterfaces().Contains(typeof(IList)) && t.IsGenericType && t.GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool ValidIndex(this IList list, int index) => (index >= 0) && (index < list.Count);

        public static bool SameElementsAs<T>(this IEnumerable<T> set1, IEnumerable<T> set2) =>
            (set1.Count() == set2.Count()) &&
            !set1.Except(set1).Any() &&
            !set2.Except(set1).Any();
    }
}
