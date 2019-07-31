using System;
using System.Collections.Generic;
using UnityEngine;

using static B9PartSwitch.Localization;
using GroupedStringBuilder = B9PartSwitch.Utils.GroupedStringBuilder;

namespace B9PartSwitch.UI
{
    public class SwitcherSubtypeDescriptionGenerator
    {
        private const string BAD_CHANGE_COLOR = "#ff3f3f";
        private const string GOOD_CHANGE_COLOR = "#3fff3f";

        private static readonly GroupedStringBuilder stringBuilder = new GroupedStringBuilder();
        private static readonly object lockObject = new object();

        private readonly ModuleB9PartSwitch module;

        private readonly float partDryMass, partWetMass, partDryCost, partWetCost, baseDryMass, baseWetMass, baseDryCost, baseWetCost;
        private readonly bool showDryMass, showWetMass, showDryCost, showWetCost, showMaxTemp, showSkinMaxTemp, showCrashTolerance;
        private readonly float prefabMaxTemp, prefabSkinMaxTemp, prefabCrashTolerance, currentMaxTemp, currentSkinMaxTemp, currentCrashTolerance;

        private readonly KeyValuePair<TankResource, float>[] parentResources;
        private readonly float baseParentVolume;

        public SwitcherSubtypeDescriptionGenerator(ModuleB9PartSwitch module)
        {
            module.ThrowIfNullArgument(nameof(module));
            this.module = module;

            Part partPrefab = module.part.GetPrefab();

            float prefabMass = partPrefab.mass;
            partDryMass = prefabMass + module.part.GetModuleMass(prefabMass);
            partWetMass = partDryMass + module.part.GetResourceMassMax();

            baseDryMass = partDryMass - module.GetDryMass(module.CurrentSubtype) - module.GetParentDryMass(module.CurrentSubtype);
            baseWetMass = partWetMass - module.GetWetMass(module.CurrentSubtype) - module.GetParentWetMass(module.CurrentSubtype);

            float prefabCost = module.part.partInfo.cost;
            partWetCost = prefabCost + module.part.GetModuleCosts(prefabCost);
            partDryCost = partWetCost - module.part.GetResourceCostMax();

            baseDryCost = partDryCost - module.GetDryCost(module.CurrentSubtype) - module.GetParentDryCost(module.CurrentSubtype);
            baseWetCost = partWetCost - module.GetWetCost(module.CurrentSubtype) - module.GetParentWetCost(module.CurrentSubtype);

            showWetMass = module.ChangesResourceMass;
            showWetMass |= module.Parent?.CurrentTankType.ChangesResourceMass ?? false;

            showDryMass = showWetMass;
            showDryMass |= module.ChangesDryMass;
            showDryMass |= (module.Parent?.CurrentTankType.tankMass ?? 0) != 0;

            showWetCost = module.ChangesResourceCost;
            showWetCost |= module.Parent?.CurrentTankType.ChangesResourceCost ?? false;

            showDryCost = showWetCost;
            showDryCost |= module.ChangesDryCost;
            showDryCost |= (module.Parent?.CurrentTankType.tankCost ?? 0) != 0;

            showMaxTemp = module.HasPartAspectLock("maxTemp");
            showSkinMaxTemp = module.HasPartAspectLock("skinMaxTemp");
            showCrashTolerance = module.HasPartAspectLock("crashTolerance");

            prefabMaxTemp = (float)partPrefab.maxTemp;
            prefabSkinMaxTemp = (float)partPrefab.skinMaxTemp;
            prefabCrashTolerance = partPrefab.crashTolerance;

            currentMaxTemp = (float)module.part.maxTemp;
            currentSkinMaxTemp = (float)module.part.skinMaxTemp;
            currentCrashTolerance = module.part.crashTolerance;

            float currentParentVolume = module.Parent?.GetTotalVolume(module.Parent.CurrentSubtype) ?? 0;
            baseParentVolume = currentParentVolume - (module.CurrentSubtype.volumeAddedToParent * module.VolumeScale);

            parentResources = new KeyValuePair<TankResource, float>[module.Parent?.CurrentTankType.resources.Count ?? 0];

            for (int i = 0; i < parentResources.Length; i++)
            {
                TankResource resource = module.Parent.CurrentTankType[i];
                parentResources[i] = new KeyValuePair<TankResource, float>(resource, resource.unitsPerVolume * currentParentVolume);
            }
        }

        public string GetFullSubtypeDescription(PartSubtype subtype)
        {
            lock (lockObject)
            {
                try
                {
                    GetFullSubtypeDescriptionInternal(subtype);
                    string result = stringBuilder.ToString();
                    int leadingWhitespace = result.Length - result.TrimStart().Length;
                    int trailingWhitespace = result.Length - result.TrimEnd().Length;
                    if (leadingWhitespace != 0)
                        Debug.LogError("[SwitcherSubtypeDescriptionGenerator] decription has leading whitespace: " + leadingWhitespace);
                    if (trailingWhitespace != 0)
                        Debug.LogError("[SwitcherSubtypeDescriptionGenerator] decription has trailing whitespace: " + trailingWhitespace);
                    return result;
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    return "<color=orange><b>error generating description</b></color>\n\nhere's some placeholder text instead";
                }
                finally
                {
                    stringBuilder.Clear();
                }
            }
        }

        private void GetFullSubtypeDescriptionInternal(PartSubtype subtype)
        {
            if (!subtype.descriptionSummary.IsNullOrEmpty())
            {
                stringBuilder.AppendLine(subtype.descriptionSummary);
            }

            stringBuilder.BeginGroup();

            if (subtype.tankType.resources.Count > 0 || parentResources.Length > 0)
            {
                stringBuilder.AppendLine("<b>{0}:</b>", SwitcherSubtypeDescriptionGenerator_Resources);
                foreach (TankResource resource in subtype.tankType)
                    stringBuilder.AppendLine("  <color=#bfff3f>- {0}</color>: {1:0.#}", resource.resourceDefinition.displayName, resource.unitsPerVolume * module.GetTotalVolume(subtype));

                float parentVolume = baseParentVolume + (subtype.volumeAddedToParent * module.VolumeScale);

                foreach (KeyValuePair<TankResource, float> resourceAndAmount in parentResources)
                {
                    float amount = resourceAndAmount.Key.unitsPerVolume * parentVolume;
                    float difference = amount - resourceAndAmount.Value;
                    stringBuilder.Append("  <color=#bfff3f>- {0}</color>: {1:0.#}", resourceAndAmount.Key.resourceDefinition.displayName, amount);
                    if (!ApproximatelyZero(difference)) stringBuilder.Append(FormatResourceDifference(difference));
                    stringBuilder.AppendLine();
                }
            }

            float dryMass = baseDryMass + module.GetDryMass(subtype) + module.GetParentDryMass(subtype);
            float wetMass = baseWetMass + module.GetWetMass(subtype) + module.GetParentWetMass(subtype);

            float dryMassDifference = dryMass - partDryMass;
            float wetMassDifference = wetMass - partWetMass;

            float dryCost = baseDryCost + module.GetDryCost(subtype) + module.GetParentDryCost(subtype);
            float wetCost = baseWetCost + module.GetWetCost(subtype) + module.GetParentWetCost(subtype);

            float dryCostDifference = dryCost - partDryCost;
            float wetCostDifference = wetCost - partWetCost;

            stringBuilder.BeginGroup();

            if (showWetMass)
            {
                stringBuilder.Append("<b>{0}:</b> {1} {2}", SwitcherSubtypeDescriptionGenerator_Mass, SwitcherSubypeDescriptionGenerator_MassTons(dryMass, "0.###"), SwitcherSubtypeDescriptionGenerator_TankEmpty);
                if (!ApproximatelyZero(dryMassDifference) && !ApproximatelyEqual(dryMassDifference, wetMassDifference)) stringBuilder.Append(FormatMassDifference(dryMassDifference));
                stringBuilder.Append(" / {0} {1}", SwitcherSubypeDescriptionGenerator_MassTons(wetMass, "0.###"), SwitcherSubtypeDescriptionGenerator_TankFull);
                if (!ApproximatelyZero(wetMassDifference)) stringBuilder.Append(FormatMassDifference(wetMassDifference));
                stringBuilder.AppendLine();
            }
            else if (showDryMass)
            {
                stringBuilder.Append("<b>{0}:</b> {1}", SwitcherSubtypeDescriptionGenerator_Mass, SwitcherSubypeDescriptionGenerator_MassTons(dryMass, "0.###"));
                if (!ApproximatelyZero(dryMassDifference)) stringBuilder.Append(FormatMassDifference(dryMassDifference));
                stringBuilder.AppendLine();
            }

            if (showWetCost)
            {
                stringBuilder.Append("<b>{0}:</b> <sprite=\"CurrencySpriteAsset\" name=\"Funds\" tint=1> {1:0.#} {2}", SwitcherSubtypeDescriptionGenerator_Cost, dryCost, SwitcherSubtypeDescriptionGenerator_TankEmpty);
                if (!ApproximatelyZero(dryCostDifference) && !ApproximatelyEqual(dryCostDifference, wetCostDifference)) stringBuilder.Append(FormatCostDifference(dryCostDifference));
                stringBuilder.Append(" / <sprite=\"CurrencySpriteAsset\" name=\"Funds\" tint=1> {0:0.#} {1}", wetCost, SwitcherSubtypeDescriptionGenerator_TankFull);
                if (!ApproximatelyZero(wetCostDifference)) stringBuilder.Append(FormatCostDifference(wetCostDifference));
                stringBuilder.AppendLine();
            }
            else if (showDryCost)
            {
                stringBuilder.Append("<b>{0}:</b> <sprite=\"CurrencySpriteAsset\" name=\"Funds\" tint=1> {1:#.#}", SwitcherSubtypeDescriptionGenerator_Cost, dryCost);
                if (!ApproximatelyZero(dryCostDifference)) stringBuilder.Append(FormatCostDifference(dryCostDifference));
                stringBuilder.AppendLine();
            }

            if (showMaxTemp)
            {
                float maxTemp = subtype.maxTemp > 0 ? subtype.maxTemp : prefabMaxTemp;
                stringBuilder.Append("<b>{0}:</b> {1}", SwitcherSubtypeDescriptionGenerator_MaxTemp, SwitcherSubypeDescriptionGenerator_TemperatureKelvins(maxTemp, "#"));
                if (!ApproximatelyEqual(maxTemp, currentMaxTemp)) stringBuilder.Append(FormatTemperatureDifference(maxTemp - currentMaxTemp));
                stringBuilder.AppendLine();
            }

            if (showSkinMaxTemp)
            {
                float skinMaxTemp = subtype.skinMaxTemp > 0 ? subtype.skinMaxTemp : prefabSkinMaxTemp;
                stringBuilder.Append("<b>{0}:</b> {1}", SwitcherSubtypeDescriptionGenerator_MaxSkinTemp, SwitcherSubypeDescriptionGenerator_TemperatureKelvins(skinMaxTemp, "#"));
                if (!ApproximatelyEqual(skinMaxTemp, currentSkinMaxTemp)) stringBuilder.Append(FormatTemperatureDifference(skinMaxTemp - currentSkinMaxTemp));
                stringBuilder.AppendLine();
            }

            if (showCrashTolerance)
            {
                float crashTolerance = subtype.crashTolerance > 0 ? subtype.crashTolerance : prefabCrashTolerance;
                stringBuilder.Append("<b>{0}:</b> {1}", SwitcherSubtypeDescriptionGenerator_CrashTolerance, SwitcherSubypeDescriptionGenerator_SpeedMetersPerSecond(crashTolerance, "#"));
                if (!ApproximatelyEqual(crashTolerance, currentCrashTolerance)) stringBuilder.Append(FormatSpeedDifference(crashTolerance - currentCrashTolerance));
                stringBuilder.AppendLine();
            }

            stringBuilder.BeginGroup();

            if (!subtype.descriptionDetail.IsNullOrEmpty())
            {
                stringBuilder.AppendLine(subtype.descriptionDetail);
            }
        }

        private static bool ApproximatelyEqual(float a, float b)
        {
            return (a == b) || Mathf.Abs(a - b) < 1e-4;
        }

        private static bool ApproximatelyZero(float a) => ApproximatelyEqual(a, 0);

        private static string FormatMassDifference(float massDifference)
        {
            string color = massDifference > 0 ? BAD_CHANGE_COLOR : GOOD_CHANGE_COLOR;
            string formattedValue = SwitcherSubypeDescriptionGenerator_MassTons(massDifference, "+0.###;-0.###");
            return $" (<color={color}>{formattedValue}</color>)";
        }

        private static string FormatCostDifference(float costDifference)
        {
            string color = costDifference > 0 ? BAD_CHANGE_COLOR : GOOD_CHANGE_COLOR;
            string formattedValue = costDifference.ToString("+0.#;-0.#");
            return $" (<color={color}>{formattedValue}</color>)";
        }

        private static string FormatTemperatureDifference(float temperatureDifference)
        {
            string color = temperatureDifference > 0 ? GOOD_CHANGE_COLOR : BAD_CHANGE_COLOR;
            string formattedValue = SwitcherSubypeDescriptionGenerator_TemperatureKelvins(temperatureDifference, "+#;-#");
            return $" (<color={color}>{formattedValue}</color>)";
        }

        private static string FormatSpeedDifference(float speedDifference)
        {
            string color = speedDifference > 0 ? GOOD_CHANGE_COLOR : BAD_CHANGE_COLOR;
            string formattedValue = SwitcherSubypeDescriptionGenerator_SpeedMetersPerSecond(speedDifference, "+#;-#");
            return $" (<color={color}>{formattedValue}</color>)";
        }

        private static string FormatResourceDifference(float resourceDifference)
        {
            string color = resourceDifference > 0 ? GOOD_CHANGE_COLOR : BAD_CHANGE_COLOR;
            return $" (<color={color}>{resourceDifference:+0.#;-0.#}</color>)";
        }
    }
}
