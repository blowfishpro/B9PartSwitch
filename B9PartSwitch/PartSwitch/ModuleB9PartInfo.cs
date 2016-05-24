using System.Linq;
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

        [KSPField(guiName = "Cost (Dry)", guiFormat = "N2")]
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
            if (HighLogic.LoadedSceneIsFlight || !part.Modules.OfType<ModuleB9PartSwitch>().Any(m => m.DisplayInfo))
            {
                part.Modules.Remove(this);
                Destroy(this);
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
            bool hasResources = part.Resources.Count > 0;
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

            Fields[nameof(maxTemp)].guiActiveEditor = showInfo && switcherModules.Any(module => module.MaxTempManaged);
            Fields[nameof(skinMaxTemp)].guiActiveEditor = showInfo && switcherModules.Any(module => module.SkinMaxTempManaged);
            Fields[nameof(crashTolerance)].guiActiveEditor = showInfo && switcherModules.Any(module => module.CrashToleranceManaged);
        }

        private void UpdateFields()
        {
            dryMass = part.mass;
            wetMass = part.mass + part.GetResourceMass();

            float partCost = part.partInfo.cost + part.GetModuleCosts(part.partInfo.cost);
            wetCost = partCost + part.GetResourceCostOffset();
            dryCost = partCost - part.GetResourceCostMax();

            maxTemp = (float)part.maxTemp;
            skinMaxTemp = (float)part.skinMaxTemp;
            crashTolerance = part.crashTolerance;
        }
    }
}
