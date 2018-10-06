using UniLinq;
using System.Collections.Generic;
using KSP.UI.Screens.Editor;
using UnityEngine;
using KSP.UI;

namespace B9PartSwitch
{
    public class ModuleB9PartInfo : PartModule
    {
        public const string DryMassGUIString = "Mass (Dry)";
        public const string MassGUIString = "Mass";

        public const string DryCostGUIString = "Cost (Dry)";
        public const string CostGUIString = "Cost";

        [KSPEvent(guiActiveEditor = true)]
        public void ShowToolTip()
        {
            GameObject screen = new GameObject();
            RectTransform transform = screen.AddComponent<RectTransform>();
            Canvas canvas = screen.AddComponent<Canvas>();
            KSPGraphicRaycaster rayCaster = screen.AddComponent<KSPGraphicRaycaster>();
            rayCaster.blockingObjects = UnityEngine.UI.GraphicRaycaster.BlockingObjects.All;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;


            PartListTooltip prefab = FindObjectOfType<PartListTooltipController>().tooltipPrefab;
            //PartListTooltip tooltip = Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
            PartListTooltip tooltip = Instantiate(prefab,transform);
            tooltip.gameObject.GetChild("ThumbContainer").DestroyGameObject();

            AvailablePart ap = new AvailablePart();
            ap.name = part.partInfo.name;
            ap.title = part.partInfo.title;
            ap.manufacturer = part.partInfo.manufacturer;
            ap.author = part.partInfo.author;
            ap.description = part.partInfo.description;
            ap.typeDescription = part.partInfo.typeDescription;
            ap.moduleInfo = part.partInfo.moduleInfo;
            ap.resourceInfo = part.partInfo.resourceInfo;
            ap.category = part.partInfo.category;
            ap.TechHidden = part.partInfo.TechHidden;
            ap.amountAvailable = part.partInfo.amountAvailable;
            ap.cost = part.partInfo.cost;
            ap.bulkheadProfiles = part.partInfo.bulkheadProfiles;
            ap.tags = part.partInfo.tags;
            ap.partUrl = part.partInfo.partUrl;
            ap.iconPrefab = part.partInfo.iconPrefab;
            ap.iconScale = part.partInfo.iconScale;
            ap.partPrefab = part;
            ap.internalConfig = part.partInfo.internalConfig;
            ap.partConfig = part.partInfo.partConfig;
            ap.configFileFullName = part.partInfo.configFileFullName;
            ap.partUrlConfig = part.partInfo.partUrlConfig;
            ap.fileTimes = new List<string>(part.partInfo.fileTimes);
            ap.partSize = part.partInfo.partSize;
            ap.TechRequired = part.partInfo.TechRequired;
            ap.identicalParts = part.partInfo.identicalParts;
            ap.moduleInfos = new List<AvailablePart.ModuleInfo>(part.partInfo.moduleInfos);
            ap.resourceInfos = new List<AvailablePart.ResourceInfo>(part.partInfo.resourceInfos);
            ap.variant = part.partInfo.variant;
            ap.showVesselNaming = part.partInfo.showVesselNaming;
            tooltip.Setup(ap, null);
            tooltip.DisplayExtendedInfo(true, "hint text");

            screen.SetLayerRecursive(LayerMask.NameToLayer("UI"));

            Canvas.ForceUpdateCanvases();
            UIMasterController.RepositionTooltip((RectTransform)tooltip.transform, Vector2.one, 8f);
            //PartListTooltipController. -> component of the each part icon in the part list, has reference to the partlisttooltip prefab
        }

        [UI_Toggle(enabledText = "Enabled", disabledText = "Hidden")]
        [KSPField(guiActiveEditor = true, guiName = "Part Info")]
        public bool showInfo = false;

        [KSPField(guiName = DryMassGUIString, guiFormat = "N3", guiUnits = "t")]
        public float dryMass = 0f;

        [KSPField(guiName = "Mass (Wet)", guiFormat = "N3", guiUnits = "t")]
        public float wetMass = 0f;

        [KSPField(guiName = DryCostGUIString, guiFormat = "N2")]
        public float dryCost = 0f;

        [KSPField(guiName = "Cost (Wet)", guiFormat = "N2")]
        public float wetCost = 0f;

        [KSPField(guiName = "Max Temp", guiFormat = "F0", guiUnits = "K")]
        public float maxTemp;

        [KSPField(guiName = "Max Skin Temp", guiFormat = "F0", guiUnits = "K")]
        public float skinMaxTemp;

        [KSPField(guiName = "Crash Tolerance", guiFormat = "F0", guiUnits = "m/s")]
        public float crashTolerance;

        private void Start()
        {
            if (HighLogic.LoadedSceneIsFlight || !part.Modules.OfType<ModuleB9PartSwitch>().Any(m => DisplayInfoOnSwitcher(m)))
            {
                Fields[nameof(showInfo)].guiActiveEditor = false;
                return;
            }

            SetupGUI();
            UpdateFields();

            GameEvents.onEditorShipModified.Add(EditorShipModified);
        }

        private void EditorShipModified(ShipConstruct construct)
        {
            SetupGUI();
            UpdateFields();
        }

        private void SetupGUI()
        {
            List<ModuleB9PartSwitch> switcherModules = part.FindModulesImplementing<ModuleB9PartSwitch>();
            bool hasResources = part.Resources.Any(resource => resource.info.density != 0f);
            bool showMass = switcherModules.Any(module => module.ChangesMass);
            bool showCost = switcherModules.Any(module => module.ChangesCost);
            bool hasInfo = switcherModules.Any(module => DisplayInfoOnSwitcher(module));

            Fields[nameof(showInfo)].guiActiveEditor = hasResources || hasInfo;

            var dryMassField = Fields[nameof(dryMass)];
            dryMassField.guiActiveEditor = showInfo && showMass;
            dryMassField.guiName = hasResources ? DryMassGUIString : MassGUIString;
            Fields[nameof(wetMass)].guiActiveEditor = showInfo && showMass && hasResources;

            var dryCostField = Fields[nameof(dryCost)];
            dryCostField.guiActiveEditor = showInfo && showCost;
            dryCostField.guiName = hasResources ? DryCostGUIString : CostGUIString;
            Fields[nameof(wetCost)].guiActiveEditor = showInfo && showCost && hasResources;

            Fields[nameof(maxTemp)].guiActiveEditor = showInfo && switcherModules.Any(module => module.PartFieldManaged(SubtypePartFields.MaxTemp));
            Fields[nameof(skinMaxTemp)].guiActiveEditor = showInfo && switcherModules.Any(module => module.PartFieldManaged(SubtypePartFields.SkinMaxTemp));
            Fields[nameof(crashTolerance)].guiActiveEditor = showInfo && switcherModules.Any(module => module.PartFieldManaged(SubtypePartFields.CrashTolerance));
        }

        private void UpdateFields()
        {
            float prefabMass = part.GetPrefab().mass;
            dryMass = prefabMass + part.GetModuleMass(prefabMass);
            wetMass = dryMass + part.GetResourceMass();

            float partCost = part.partInfo.cost + part.GetModuleCosts(part.partInfo.cost);
            wetCost = partCost + part.GetResourceCostOffset();
            dryCost = partCost - part.GetResourceCostMax();

            maxTemp = (float)part.maxTemp;
            skinMaxTemp = (float)part.skinMaxTemp;
            crashTolerance = part.crashTolerance;
        }

        private bool DisplayInfoOnSwitcher(ModuleB9PartSwitch switcher)
        {
            return
                switcher.ChangesMass ||
                switcher.ChangesCost ||
                switcher.PartFieldManaged(SubtypePartFields.MaxTemp) ||
                switcher.PartFieldManaged(SubtypePartFields.SkinMaxTemp) ||
                switcher.PartFieldManaged(SubtypePartFields.CrashTolerance);
        }
    }
}
