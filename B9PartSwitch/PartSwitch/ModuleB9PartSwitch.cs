using System;
using System.Collections.Generic;
using System.Linq;
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

        [ConfigField(persistant = true)]
        public int currentSubtypeIndex = 0;

        [ConfigField]
        public bool affectDragCubes = true;

        [ConfigField]
        public bool affectFARVoxels = true;

        // Can't use built-in symmetry because it doesn't know how to find the correct module on the other part
        [KSPField(guiActiveEditor = true, isPersistant = true, guiName = "Subtype")]
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, scene = UI_Scene.Editor, suppressEditorShipModified = true)]
        public int subtypeIndexControl = 0;

        #endregion

        #region Private Fields

        [SerializeField]
        private List<string> managedResourceNames = new List<string>();

        [SerializeField]
        private List<string> managedTransformNames = new List<string>();

        [SerializeField]
        private List<string> managedStackNodeIDs = new List<string>();

        // Tweakscale integration
        private float scale = 1f;

        #endregion

        #region Properties

        public int SubtypesCount => subtypes.Count;

        public PartSubtype CurrentSubtype => subtypes[currentSubtypeIndex];

        public TankType CurrentTankType => CurrentSubtype.tankType;

        public float CurrentVolume => CurrentSubtype.TotalVolume * VolumeScale;

        public PartSubtype this[int index] => subtypes[index];

        public bool ManagesResources => subtypes.Any(s => !s.tankType.IsStructuralTankType);

        public bool MaxTempManaged { get; private set; }
        public bool SkinMaxTempManaged { get; private set; }
        public bool AttachNodeManaged { get; private set; }

        public float VolumeScale => scale * scale * scale;

        #endregion

        #region Setup

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            // This will deactivate objects before the part icon is created, avoiding a visual mess
            for (int i = 0; i < subtypes.Count; i++)
            {
                PartSubtype subtype = subtypes[i];
                subtype.SetParent(this);
                subtype.FindObjects();

                subtype.DeactivateObjects();
            }

            CurrentSubtype.ActivateObjects();
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            SetupSubtypes();

            if (currentSubtypeIndex >= subtypes.Count || currentSubtypeIndex < 0)
                currentSubtypeIndex = 0;

            SetupGUI();

            foreach (var subtype in subtypes)
            {
                if (subtype == CurrentSubtype)
                    continue;

                subtype.DeactivateObjects();
                if (state == StartState.Editor)
                    subtype.DeactivateNodes();
                else
                    subtype.ActivateNodes();
            }

            UpdateSubtype(false);
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

                if (otherModule.MaxTempManaged && MaxTempManaged)
                {
                    LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot both manage the part's maxTemp");
                    destroy = true;
                }

                if (otherModule.SkinMaxTempManaged && SkinMaxTempManaged)
                {
                    LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot both manage the part's skinMaxTemp");
                    destroy = true;
                }

                if (otherModule.AttachNodeManaged && AttachNodeManaged)
                {
                    LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot both manage the part's attach node");
                    destroy = true;
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
                    modifiedSetup = true;
                }

            }

            // If there were incompatible modules, they might have messed with things
            if (modifiedSetup)
            {
                UpdateSubtype(false);
            }
        }

        #endregion

        #region Interface Methods

        public float GetModuleMass(float baseMass, ModifierStagingSituation situation)
        {
            return (CurrentSubtype.addedMass + (CurrentVolume * CurrentTankType.tankMass)) * VolumeScale;
        }

        public ModifierChangeWhen GetModuleMassChangeWhen() => ModifierChangeWhen.FIXED;

        public float GetModuleCost(float baseCost, ModifierStagingSituation situation)
        {
            float cost = CurrentSubtype.addedCost;
            cost += (CurrentTankType.tankCost + CurrentTankType.ResourceUnitCost) * CurrentVolume;
            return cost * VolumeScale;
        }

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

        #endregion

        #region Private Methods

        private void SetupSubtypes()
        {
            managedResourceNames = new List<string>();
            managedTransformNames = new List<string>();
            managedStackNodeIDs = new List<string>();

            MaxTempManaged = false;
            SkinMaxTempManaged = false;
            AttachNodeManaged = false;

            foreach (var subtype in subtypes)
            {
                subtype.SetParent(this);
                subtype.OnStart();
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

                if (subtype.maxTemp > 0f)
                    MaxTempManaged = true;
                if (subtype.skinMaxTemp > 0f)
                    SkinMaxTempManaged = true;
                if (subtype.attachNode.IsNotNull())
                {
                    if (part.attachRules.allowSrfAttach && part.srfAttachNode != null)
                    {
                        AttachNodeManaged = true;
                    }
                    else
                    {
                        LogError($"Error: Part subtype '{subtype.Name}' has an attach node defined, but part does not allow surface attachment (or the surface attach node could not be found)");
                    }
                }
            }
        }

        private void SetupGUI()
        {
            var chooseField = Fields[nameof(subtypeIndexControl)];
            chooseField.guiName = switcherDescription;

            var chooseOption = chooseField.uiControlEditor as UI_ChooseOption;
            chooseOption.options = subtypes.Select(s => s.title).ToArray();

            chooseOption.onFieldChanged = UpdateFromGUI;
        }

        private void UpdateFromGUI(BaseField field, object oldFieldValueObj)
        {
            BeginSubtypeChange(subtypeIndexControl);
        }

        private void UpdateDragCubesOnAttach()
        {
            part.OnEditorAttach -= UpdateDragCubesOnAttach;
            RenderProceduralDragCubes();
        }

        private void BeginSubtypeChange(int newIndex)
        {
            if (newIndex < 0 || newIndex >= subtypes.Count)
                throw new ArgumentException($"Subtype index must be between 0 and {subtypes.Count}");

            // For symmetry
            subtypeIndexControl = newIndex;

            if (newIndex == currentSubtypeIndex)
                return;

            SetNewSubtype(newIndex);

            foreach (var counterpart in this.FindSymmetryCounterparts())
                counterpart.SetNewSubtype(newIndex);

            FireEvents();
        }

        private void SetNewSubtype(int newIndex)
        {
            CurrentSubtype.DeactivateObjects();
            if (HighLogic.LoadedSceneIsEditor)
                CurrentSubtype.DeactivateNodes();

            currentSubtypeIndex = newIndex;

            UpdateSubtype(true);
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

        private void UpdateSubtype(bool fillTanks)
        {
            CurrentSubtype.ActivateObjects();
            if (HighLogic.LoadedSceneIsEditor)
                CurrentSubtype.ActivateNodes();

            UpdateTankSetup(fillTanks);

            if (MaxTempManaged)
            {
                if (CurrentSubtype.maxTemp > 0)
                    part.maxTemp = CurrentSubtype.maxTemp;
                else
                    part.maxTemp = part.GetPrefab().maxTemp;
            }

            if (SkinMaxTempManaged)
            {
                if (CurrentSubtype.skinMaxTemp > 0)
                    part.skinMaxTemp = CurrentSubtype.skinMaxTemp;
                else
                    part.skinMaxTemp = part.GetPrefab().skinMaxTemp;
            }

            if (AttachNodeManaged && part.attachRules.allowSrfAttach && part.srfAttachNode != null)
            {
                var referenceNode = CurrentSubtype.attachNode ?? part.GetPrefab().srfAttachNode;
                part.srfAttachNode.position = referenceNode.position * scale;
                part.srfAttachNode.orientation = referenceNode.orientation;
                // part.srfAttachNode.size = referenceNode.size;
            }

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

            var window = FindObjectsOfType<UIPartActionWindow>().FirstOrDefault(w => w.part == part);
            if (window.IsNotNull())
                window.displayDirty = true;

            LogInfo($"Switched subtype to {CurrentSubtype.Name}");
        }

        private void UpdateTankSetup(bool forceFull)
        {
            List<PartResource> partResources = part.Resources.list;
            int[] resourceIndices = Enumerable.Repeat<int>(-1, CurrentTankType.resources.Count).ToArray();
            bool tmp = false;

            for (int i = 0; i < partResources.Count; i++)
            {
                string resourceName = partResources[i].resourceName;
                tmp = false;

                for (int j = 0; j < CurrentTankType.resources.Count; j++)
                {
                    if (resourceName == CurrentTankType.resources[j].ResourceName)
                    {
                        resourceIndices[j] = i;
                        tmp = true;
                        break;
                    }
                }

                if (tmp)
                    continue;

                if (IsManagedResource(resourceName))
                {
                    DestroyImmediate(partResources[i]);
                    partResources.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < CurrentTankType.resources.Count; i++)
            {
                TankResource resource = CurrentTankType[i];
                float resourceAmount = resource.unitsPerVolume * CurrentVolume;
                PartResource partResource = null;
                if (resourceIndices[i] < 0)
                {
                    partResource = part.gameObject.AddComponent<PartResource>();
                    partResource.SetInfo(resource.resourceDefinition);
                    partResource.maxAmount = resourceAmount;
                    partResource.amount = resourceAmount;
                    partResource.flowState = true;
                    partResource.isTweakable = resource.resourceDefinition.isTweakable;
                    partResource.hideFlow = false;
                    partResource.flowMode = PartResource.FlowMode.Both;
                    partResources.Add(partResource);
                }
                else
                {
                    partResource = part.Resources[resourceIndices[i]];
                    partResource.maxAmount = resourceAmount;
                    if (forceFull)
                    {
                        partResource.amount = resourceAmount;
                    }
                    else
                    {
                        if (partResource.amount > resourceAmount)
                            partResource.amount = resourceAmount;
                    }
                }
            }

            part.Resources.UpdateList();
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
