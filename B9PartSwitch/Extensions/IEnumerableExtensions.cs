
using System.Collections.Generic;

namespace B9PartSwitch
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> All<T>(this IEnumerable<T> enumerable)
        {
            foreach (T item in enumerable)
            {
                yield return item;
            }
        }
    }
}
