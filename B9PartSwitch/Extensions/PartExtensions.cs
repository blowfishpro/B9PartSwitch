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

        public static PartResource AddResource(this Part part, PartResourceDefinition info, float amount, float maxAmount)
        {
            PartResource resource = part.gameObject.AddComponent<PartResource>();
            resource.SetInfo(info);
            resource.maxAmount = maxAmount;
            resource.amount = amount;
            resource.flowState = true;
            resource.isTweakable = info.isTweakable;
            resource.hideFlow = false;
            resource.flowMode = PartResource.FlowMode.Both;
            part.Resources.list.Add(resource);

            return resource;
        }
        
        public static float GetResourceCostMax(this Part part) => part.Resources.list.Sum(resource => (float)resource.maxAmount * resource.info.unitCost);
        public static float GetResourceCostOffset(this Part part) => part.Resources.list.Sum(resource => (float)(resource.amount - resource.maxAmount) * resource.info.unitCost);
    }
}
