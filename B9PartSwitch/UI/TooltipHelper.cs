using System;
using UnityEngine;
using KSP.UI;
using KSP.UI.TooltipTypes;

namespace B9PartSwitch.UI
{
    public static class TooltipHelper
    {
        private static Tooltip subtypeInfoTooltip;

        public static void EnsurePrefabs()
        {
            if (subtypeInfoTooltip.IsNotNull()) return;

            subtypeInfoTooltip = CreateSubtypeInfoTooltipPrefab();
            Debug.Log("[B9PartSwitch.UI.TooltipHelper] created subtype info tooltip prefab");

        }

        public static TooltipController_TitleAndText SetupSubtypeInfoTooltip(GameObject gameObject, string titleString, string textString)
        {
            TooltipController_TitleAndText tooltipController = gameObject.AddOrGetComponent<TooltipController_TitleAndText>();
            tooltipController.TooltipPrefabType = subtypeInfoTooltip;

            tooltipController.titleString = titleString;
            tooltipController.textString = textString;
            return tooltipController;
        }

        public static void SetupSubtypeInfoTooltip(TooltipController_TitleAndText tooltipController, string titleString, string textString)
        {
            tooltipController.TooltipPrefabType = subtypeInfoTooltip;

            tooltipController.titleString = titleString;
            tooltipController.textString = textString;
        }

        private static Tooltip CreateSubtypeInfoTooltipPrefab()
        {
            Tooltip tooltipPrefab_TitleAndText = AssetBase.GetPrefab<Tooltip>("Tooltip_TitleAndText");
            GameObject subtypeInfoTooltipPrefabGameObject = UnityEngine.Object.Instantiate(tooltipPrefab_TitleAndText.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(subtypeInfoTooltipPrefabGameObject);
            subtypeInfoTooltipPrefabGameObject.GetChild("Text").GetComponent<TMPro.TextMeshProUGUI>().alpha = 0.9f;
            subtypeInfoTooltipPrefabGameObject.GetChild("Title").GetComponent<UnityEngine.UI.LayoutElement>().minWidth = 300;

            return subtypeInfoTooltipPrefabGameObject.GetComponent<Tooltip>();
        }
    }
}
