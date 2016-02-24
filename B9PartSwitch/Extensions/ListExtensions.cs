using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

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
    }
}
