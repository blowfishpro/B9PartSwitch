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
        
        public static float GetResourceCostMax(this Part part) => part.Resources.list.Sum(resource => (float)resource.maxAmount * resource.info.unitCost);
        public static float GetResourceCostOffset(this Part part) => part.Resources.list.Sum(resource => (float)(resource.amount - resource.maxAmount) * resource.info.unitCost);
    }
}
