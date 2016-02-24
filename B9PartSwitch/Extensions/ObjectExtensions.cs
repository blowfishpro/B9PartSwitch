using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace B9PartSwitch
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this Object o)
        {
            return (o == null);
        }

        public static bool IsNotNull(this Object o)
        {
            return (o != null);
        }
    }
}
