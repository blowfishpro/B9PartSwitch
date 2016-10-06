using UniLinq;
using UnityEngine;

namespace B9PartSwitch
{
    public static class PartExtensions
    {
        public static Part GetPrefab(this Part part)
        {
            return part.partInfo.partPrefab;
        }

        public static PartResource AddResource(this Part part, PartResourceDefinition info, float maxAmount, float amount)
        {
            PartResource resource = new PartResource(part);
            resource.SetInfo(info);
            resource.maxAmount = maxAmount;
            resource.amount = amount;
            resource.flowState = true;
            resource.isTweakable = info.isTweakable;
            resource.isVisible = info.isVisible;
            resource.hideFlow = false;
            resource.flowMode = PartResource.FlowMode.Both;
            part.Resources.dict.Add(info.name.GetHashCode(), resource);

            return resource;
        }

        // Amount < 0 signifies use existing amount if exists, or create with max amount
        public static PartResource AddOrCreateResource(this Part part, PartResourceDefinition info, float maxAmount, float amount)
        {
            if (amount > maxAmount)
            {
                part.LogWarning($"Cannot add resource '{info.name}' with amount > maxAmount, will use maxAmount (amount = {amount}, maxAmount = {maxAmount})");
                amount = maxAmount;
            }

            PartResource resource = part.Resources[info.name];

            if (resource == null)
            {
                if (amount < 0f)
                    amount = maxAmount;

                resource = part.AddResource(info, maxAmount, amount);
            }
            else
            {
                resource.maxAmount = maxAmount;

                if (amount >= 0f)
                    resource.amount = amount;
            }

            return resource;
        }

        public static float GetResourceCostMax(this Part part) => part.Resources.Sum(resource => (float)resource.maxAmount * resource.info.unitCost);
        public static float GetResourceCostOffset(this Part part) => part.Resources.Sum(resource => (float)(resource.amount - resource.maxAmount) * resource.info.unitCost);

        public static void LogInfo(this Part part, object message) => Debug.Log($"[Part {part.name}] {message}");
        public static void LogWarning(this Part part, object message) => Debug.LogWarning($"[WARNING] [Part {part.name}] {message}");
        public static void LogError(this Part part, object message) => Debug.LogError($"[ERROR] [Part {part.name}] {message}");
    }
}
