using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace B9PartSwitch
{
    public class ModuleB9PartSwitch : CFGUtilPartModule, IPartMassModifier, IPartCostModifier, IModuleInfo
    {
        #region Public Fields

        [ConfigNodeSerialized]
        [ConfigField(configName = "SUBTYPE")]
        public List<PartSubtype> subtypes = new List<PartSubtype>();

        [ConfigField]
        public float baseVolume = 0f;

        [ConfigField]
        public string switcherDescription = "Subtype";

        [ConfigField]
        public string switcherDescriptionPlural = "Subtypes";

        [ConfigField]
        public bool affectDragCubes = true;

        [ConfigField]
        public bool affectFARVoxels = true;

        [ConfigField(persistant = true)]
        public string currentSubtypeName = null;

        // Can't use built-in symmetry because it doesn't know how to find the correct module on the other part
        [KSPField(guiActiveEditor = true, isPersistant = true, guiName = "Subtype")]
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, scene = UI_Scene.Editor, suppressEditorShipModified = true)]
        public int currentSubtypeIndex = -1;

        #endregion

        #region Private Fields

        private List<string> managedResourceNames = new List<string>();
        private List<string> managedTransformNames = new List<string>();
        private List<string> managedStackNodeIDs = new List<string>();

        // Tweakscale integration
        private float scale = 1f;

        #endregion

        #region Properties

        public int SubtypesCount => subtypes.Count;

        // Provide a default of zero in case best subtype has not yet been determined
        public int SubtypeIndex => subtypes.ValidIndex(currentSubtypeIndex) ? currentSubtypeIndex : 0;
        public PartSubtype CurrentSubtype => subtypes[SubtypeIndex];

        public TankType CurrentTankType => CurrentSubtype.tankType;

        public float CurrentVolume => CurrentSubtype.TotalVolume * VolumeScale;

        public PartSubtype this[int index] => subtypes[index];

        public bool ManagesResources => subtypes.Any(s => !s.tankType.IsStructuralTankType);
        public bool ChangesMass => subtypes.Any(s => s.ChangesMass);
        public bool ChangesCost => subtypes.Any(s => s.ChangesCost);

        public float Scale => scale;
        public float VolumeScale => scale * scale * scale;

        #endregion

        #region Setup

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            // This will deactivate objects before the part icon is created, avoiding a visual mess

            if (!this.ParsedPrefab())
                SetupForIcon();
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            SetupSubtypes();

            FindBestSubtype();

            SetupGUI();

            UpdateOnStart();
        }

        // This runs after OnStart() so everything should be initalized
        public void Start()
        {
            // Check for incompatible modules
            bool modifiedSetup = false;
            
            foreach (var otherModule in part.Modules.OfType<ModuleB9PartSwitch>())
            {
                if (otherModule == this) continue;
                bool destroy = false;
                foreach (var resourceName in managedResourceNames)
                {
                    if (otherModule.IsManagedResource(resourceName))
                    {
                        LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot manage the same resource: {resourceName}");
                        destroy = true;
                    }
                }
                foreach (var transformName in managedTransformNames)
                {
                    if (otherModule.IsManagedTransform(transformName))
                    {
                        LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot manage the same transform: {transformName}");
                        destroy = true;
                    }
                }
                foreach (var nodeID in managedStackNodeIDs)
                {
                    if (otherModule.IsManagedNode(nodeID))
                    {
                        LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot manage the same attach node: {nodeID}");
                        destroy = true;
                    }
                }

                foreach (ISubtypePartField field in SubtypePartFields.All)
                {
                    if (PartFieldManaged(field) && otherModule.PartFieldManaged(field))
                    {
                        LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot both manage the part's {field.Name}");
                        destroy = true;
                    }
                }

                if (destroy)
                {
                    LogWarning($"{nameof(ModuleB9PartSwitch)} with moduleID '{otherModule.moduleID}' is incomatible, and will be removed.");
                    part.RemoveModule(otherModule);
                    modifiedSetup = true;
                }
            }

            if (ManagesResources)
            {
                bool incompatible = false;
                string[] incompatibleModules = { "FSfuelSwitch", "InterstellarFuelSwitch", "ModuleFuelTanks" };
                foreach (var moduleName in incompatibleModules.Where(modName => part.Modules.Contains(modName)))
                {
                    LogError($"{nameof(ModuleB9PartSwitch)} and {moduleName} cannot both manage resources on the same part.  {nameof(ModuleB9PartSwitch)} will not manage resources.");
                    incompatible = true;
                }

                if (incompatible)
                {
                    foreach (var subtype in subtypes)
                    {
                        if (!subtype.tankType.IsStructuralTankType)
                            subtype.tankType = B9TankSettings.StructuralTankType;
                    }
                    managedResourceNames.Clear();
                    modifiedSetup = true;
                }

            }

            // If there were incompatible modules, they might have messed with things
            if (modifiedSetup)
            {
                UpdateOnStart();
            }
        }

        #endregion

        #region Interface Methods

        public float GetModuleMass(float baseMass, ModifierStagingSituation situation) => CurrentSubtype.TotalMass * VolumeScale;

        public ModifierChangeWhen GetModuleMassChangeWhen() => ModifierChangeWhen.FIXED;

        public float GetModuleCost(float baseCost, ModifierStagingSituation situation) => CurrentSubtype.TotalCost * VolumeScale;

        public ModifierChangeWhen GetModuleCostChangeWhen() => ModifierChangeWhen.FIXED;

        public override string GetInfo()
        {
            string outStr = "";
            foreach (var subtype in subtypes)
            {
                outStr += $"\n<b>- {subtype.title}</b>";
                foreach (var resource in subtype.tankType)
                    outStr += $"\n  <b><color=#99ff00ff>- {resource.ResourceName}</color></b>: {resource.unitsPerVolume * subtype.TotalVolume :F1}";
            }
            return outStr;
        }

        public string GetModuleTitle() => $"Switchable Part ({SubtypesCount} {switcherDescriptionPlural})";

        public string GetPrimaryField()
        {
            string outStr = $"<b>{subtypes.Count} {switcherDescriptionPlural}</b>";
            if (baseVolume > 0)
                outStr += $" (<b>Volume:</b> {baseVolume :F0})";
            return outStr;
        }

        public Callback<Rect> GetDrawModulePanelCallback() => null;

        #endregion

        #region Public Methods

        public bool IsManagedResource(string resourceName)
        {
            return managedResourceNames.Contains(resourceName);
        }

        public bool IsManagedTransform(string transformName)
        {
            return managedTransformNames.Contains(transformName);
        }

        public bool IsManagedNode(string nodeName)
        {
            return managedStackNodeIDs.Contains(nodeName);
        }

        public bool PartFieldManaged(ISubtypePartField field) => subtypes.Any(subtype => field.ShouldUseOnSubtype(subtype.Context));

        #endregion

        #region Private Methods

        #region Setup

        private void SetupForIcon()
        {
            foreach (var subtype in subtypes)
            {
                subtype.Setup(this);
                subtype.DeactivateObjects();
            }
            CurrentSubtype.ActivateObjects();
        }

        private void SetupSubtypes()
        {
            managedResourceNames = new List<string>();
            managedTransformNames = new List<string>();
            managedStackNodeIDs = new List<string>();

            foreach (var subtype in subtypes)
            {
                subtype.Setup(this);
                TankType tank = subtype.tankType;

                if (tank == null)
                    LogError($"Tank is null on subtype {subtype.Name}");

                if (tank.ResourcesCount > 0 && (subtype.TotalVolume <= 0f))
                {
                    LogError($"Subtype {subtype.Name} has a tank type with resources, but no volume is specifified");
                    subtype.tankType = tank = B9TankSettings.StructuralTankType;
                }

                if (tank != null)
                    managedResourceNames.AddRange(tank.ResourceNames);

                managedTransformNames.AddRange(subtype.transformNames);
                managedStackNodeIDs.AddRange(subtype.NodeIDs);
            }

            if (PartFieldManaged(SubtypePartFields.SrfAttachNode) && !part.attachRules.allowSrfAttach || part.srfAttachNode.IsNull())
            {
                LogError($"Error: One or more subtypes have an attach node defined, but part does not allow surface attachment (or the surface attach node could not be found)");
                subtypes.ForEach(subtype => subtype.attachNode = null);
            }
        }

        private void FindBestSubtype()
        {
            // First try to identify subtype by name
            if (!string.IsNullOrEmpty(currentSubtypeName))
            {
                int index = subtypes.FindIndex(subtype => subtype.Name == currentSubtypeName);

                if (index != -1)
                {
                    currentSubtypeIndex = index;
                    return;
                }
                else
                {
                    LogError($"Cannot find subtype named '{currentSubtypeName}'");
                }
            }

            // Now try to use index
            if (subtypes.ValidIndex(currentSubtypeIndex))
            {
                currentSubtypeName = CurrentSubtype.Name;
                return;
            }

            if (ManagesResources)
            {
                // Now use resources
                // This finds all the managed resources that currently exist on teh part
                string[] resourcesOnPart = managedResourceNames.Intersect(part.Resources.Select(resource => resource.resourceName)).ToArray();

#if DEBUG
                LogInfo($"Managed resources found on part: [{string.Join(", ", resourcesOnPart)}]");
#endif

                // If any of the part's current resources are managed, look for a subtype which has all of the managed resources (and all of its resources exist)
                // Otherwise, look for a structural subtype (no resources)
                if (resourcesOnPart.Any())
                {
                    currentSubtypeIndex = subtypes.FindIndex(subtype => subtype.HasTank && subtype.ResourceNames.SameElementsAs(resourcesOnPart));
                    LogInfo($"Inferred subtype based on part's resources: '{CurrentSubtype.Name}'");
                }
                else
                {
                    currentSubtypeIndex = subtypes.FindIndex(subtype => !subtype.HasTank);
                }
            }
            
            // No useful way to determine correct subtype, just pick first
            if (!subtypes.ValidIndex(currentSubtypeIndex))
                currentSubtypeIndex = 0;

            currentSubtypeName = CurrentSubtype.Name;
        }

        private void SetupGUI()
        {
            var chooseField = Fields[nameof(currentSubtypeIndex)];
            chooseField.guiName = switcherDescription;

            var chooseOption = (UI_ChooseOption)chooseField.uiControlEditor;
            chooseOption.options = subtypes.Select(s => s.title).ToArray();

            chooseOption.onFieldChanged = UpdateFromGUI;
        }

        private void UpdateOnStart()
        {
            subtypes.ForEach(subtype => subtype.DeactivateOnStart());
            RemoveUnusedResources();
            CurrentSubtype.ActivateOnStart();
            UpdateGeometry();

            LogInfo($"Switched subtype to {CurrentSubtype.Name}");
        }

        private void RemoveUnusedResources()
        {
            foreach (PartResource resource in part.Resources)
            {
                if (IsManagedResource(resource.resourceName) && !CurrentTankType.ContainsResource(resource.resourceName))
                {
                    part.RemoveResource(resource);
                }
            }
        }

        #endregion

        private void UpdateFromGUI(BaseField field, object oldFieldValueObj)
        {
            int oldIndex = (int)oldFieldValueObj;

            subtypes[oldIndex].DeactivateOnSwitch();

            currentSubtypeName = CurrentSubtype.Name;

            CurrentSubtype.ActivateOnSwitch();
            UpdateGeometry();
            LogInfo($"Switched subtype to {CurrentSubtype.Name}");

            foreach (var counterpart in this.FindSymmetryCounterparts())
                counterpart.UpdateFromSymmetry(currentSubtypeIndex);

            UpdatePartActionWindow();
            FireEvents();
        }

        private void UpdateFromSymmetry(int newIndex)
        {
            CurrentSubtype.DeactivateOnSwitch();

            currentSubtypeIndex = newIndex;
            currentSubtypeName = CurrentSubtype.Name;

            CurrentSubtype.ActivateOnSwitch();
            UpdateGeometry();
            LogInfo($"Switched subtype to {CurrentSubtype.Name}");
        }

        private void UpdateDragCubesOnAttach()
        {
            part.OnEditorAttach -= UpdateDragCubesOnAttach;
            RenderProceduralDragCubes();
        }
        
        private void FireEvents()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartTweaked, part);
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                GameEvents.onVesselWasModified.Fire(this.vessel);
            }
        }

        private void UpdateGeometry()
        {
            if (FARWrapper.FARLoaded && affectFARVoxels && managedTransformNames.Count > 0)
            {
                part.SendMessage("GeometryPartModuleRebuildMeshData");
            }

            if (affectDragCubes && managedTransformNames.Count > 0)
            {
                if (HighLogic.LoadedSceneIsEditor && part.parent == null && EditorLogic.RootPart != part)
                    part.OnEditorAttach += UpdateDragCubesOnAttach;
                else
                    RenderProceduralDragCubes();
            }
        }

        private void UpdatePartActionWindow()
        {
            var window = FindObjectsOfType<UIPartActionWindow>().FirstOrDefault(w => w.part == part);
            if (window.IsNotNull())
                window.displayDirty = true;
        }

        private void RenderProceduralDragCubes()
        {
            DragCube newCube = DragCubeSystem.Instance.RenderProceduralDragCube(part);
            part.DragCubes.ClearCubes();
            part.DragCubes.Cubes.Add(newCube);
            part.DragCubes.ResetCubeWeights();
        }

        #endregion
    }
}
