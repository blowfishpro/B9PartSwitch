using System;
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

        public static Tcollection MaxBy<Tcollection, Tcompare>(this IEnumerable<Tcollection> enumerable, Func<Tcollection, Tcompare> mapper) where Tcompare : IComparable<Tcompare>
        {
            enumerable.ThrowIfNullArgument(nameof(enumerable));
            mapper.ThrowIfNullArgument(nameof(mapper));

            IEnumerator<Tcollection> enumerator = enumerable.GetEnumerator();

            if (!enumerator.MoveNext()) throw new InvalidOperationException("Enumerable is empty!");

            Tcollection result = enumerator.Current;
            Tcompare resultValue = mapper(result);

            while (enumerator.MoveNext())
            {
                Tcollection testResult = enumerator.Current;
                Tcompare testValue = mapper(testResult);
                if (testValue.CompareTo(resultValue) <= 0) continue;
                result = testResult;
                resultValue = testValue;
            }

            return result;
        }
    }
}
