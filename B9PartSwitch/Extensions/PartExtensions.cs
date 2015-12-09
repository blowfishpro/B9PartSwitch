using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B9PartSwitch
{
    public static class PartExtensions
    {
        public static Part GetPrefab(this Part part)
        {
            return part.partInfo.partPrefab;
        }
    }
}
