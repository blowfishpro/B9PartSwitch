using UniLinq;
using System.Collections.Generic;

namespace B9PartSwitch
{
    public class ModuleB9PartInfo : PartModule
    {
        public const string DryMassGUIString = "Mass (Dry)";
        public const string MassGUIString = "Mass";

        public const string DryCostGUIString = "Cost (Dry)";
        public const string CostGUIString = "Cost";

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
                return;

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

            var dryMassField = Fields[nameof(dryMass)];
            dryMassField.guiActiveEditor = showInfo && showMass;
            dryMassField.guiName = hasResources ? DryMassGUIString : MassGUIString;
            Fields[nameof(wetMass)].guiActiveEditor = showInfo && showMass && hasResources;

            var dryCostField = Fields[nameof(dryCost)];
            dryCostField.guiActiveEditor = showInfo && showCost;
            dryCostField.guiName = hasResources ? DryCostGUIString : CostGUIString;
            Fields[nameof(wetCost)].guiActiveEditor = showInfo && showCost && hasResources;

            Fields[nameof(maxTemp)].guiActiveEditor = showInfo && switcherModules.Any(module => module.HasPartAspectLock("maxTemp"));
            Fields[nameof(skinMaxTemp)].guiActiveEditor = showInfo && switcherModules.Any(module => module.HasPartAspectLock("skinMaxTemp"));
            Fields[nameof(crashTolerance)].guiActiveEditor = showInfo && switcherModules.Any(module => module.HasPartAspectLock("crashTolerance"));
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
                switcher.HasPartAspectLock("maxTemp") ||
                switcher.HasPartAspectLock("skinMaxTemp") ||
                switcher.HasPartAspectLock("crashTolerance");
        }
    }
}
