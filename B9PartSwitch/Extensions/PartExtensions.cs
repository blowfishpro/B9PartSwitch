using System.Collections.Generic;
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

            GameEvents.onPartResourceListChange.Fire(part);

            return resource;
        }

        // Amount < 0 signifies use existing amount if exists, or create with max amount
        public static PartResource AddOrCreateResource(this Part part, PartResourceDefinition info, float maxAmount, float amount, bool modifyAmountIfPresent)
        {
            if (amount > maxAmount)
            {
                part.LogWarning($"Cannot add resource '{info.name}' with amount > maxAmount, will use maxAmount (amount = {amount}, maxAmount = {maxAmount})");
                amount = maxAmount;
            }
            else if (amount < 0f)
            {
                part.LogWarning($"Cannot add resource '{info.name}' with amount < 0, will use 0 (amount = {amount})");
                amount = 0f;
            }

            PartResource resource = part.Resources[info.name];

            if (resource == null)
            {
                resource = part.AddResource(info, maxAmount, amount);
            }
            else
            {
                resource.maxAmount = maxAmount;

                if (modifyAmountIfPresent)
                    resource.amount = amount;
            }

            return resource;
        }

        public static float GetResourceCostMax(this Part part) => part.Resources.Sum(resource => (float)resource.maxAmount * resource.info.unitCost);
        public static float GetResourceCostOffset(this Part part) => part.Resources.Sum(resource => (float)(resource.amount - resource.maxAmount) * resource.info.unitCost);

        public static void UpdateTransformEnabled(this Part part, Transform t)
        {
            bool shouldBeEnabled = true;

            foreach (ModuleB9PartSwitch module in part.Modules.OfType<ModuleB9PartSwitch>())
            {
                if (!module.TransformShouldBeEnabled(t))
                {
                    shouldBeEnabled = false;
                    break;
                }
            }

            t.gameObject.SetActive(shouldBeEnabled);
        }

        public static void UpdateNodeEnabled(this Part part, AttachNode node)
        {
            bool shouldBeEnabled = true;

            foreach (ModuleB9PartSwitch module in part.Modules.OfType<ModuleB9PartSwitch>())
            {
                if (!module.NodeShouldBeEnabled(node))
                {
                    shouldBeEnabled = false;
                    break;
                }
            }

            if (shouldBeEnabled)
                node.Unhide();
            else
                node.Hide();
        }

        public static Transform GetModelRoot(this Part part)
        {
            part.ThrowIfNullArgument(nameof(part));

            return part.partTransform.Find("model");
        }

        public static IEnumerable<Transform> GetModelTransforms(this Part part, string name)
        {
            part.ThrowIfNullArgument(nameof(part));
            name.ThrowIfNullOrEmpty(nameof(name));

            return part.GetModelRoot().GetChildrenNamedRecursive(name);
        }

        public static void FixModuleJettison(this Part part)
        {
            if (!HighLogic.LoadedSceneIsFlight) return;

            foreach (ModuleJettison module in part.Modules.OfType<ModuleJettison>())
            {
                if (
                    !module.useMultipleDragCubes ||
                    !module.isFairing ||
                    !module.decoupleEnabled ||
                    module.isJettisoned ||
                    module.jettisonTransform == null ||
                    module.jettisonTransform.root == part.gameObject.transform.root
                ) continue;
                Object.Instantiate(module.jettisonTransform, module.jettisonTransform.parent);
                module.jettisonTransform.parent = part.GetModelRoot();
                module.jettisonTransform.gameObject.SetActive(false);
            }
        }

        public static void LogInfo(this Part part, object message) => Debug.Log($"[Part {part.name}] {message}");
        public static void LogWarning(this Part part, object message) => Debug.LogWarning($"[WARNING] [Part {part.name}] {message}");
        public static void LogError(this Part part, object message) => Debug.LogError($"[ERROR] [Part {part.name}] {message}");
    }
}
