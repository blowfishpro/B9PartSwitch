using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace B9PartSwitch
{
    public class ModuleB9PartSwitch : CFGUtilPartModule, IPartMassModifier, IPartCostModifier, IModuleInfo
    {
        #region Constants

        public static readonly string[] IncompatibleModuleNames = { "ModuleProceduralFairing",
                                                                    "FSfuelSwitch",
                                                                    "FSmeshSwitch",
                                                                    "InterstallarFuelSwitch",
                                                                    "InterstellarMeshSwitch",
                                                                    "ModuleFuelTanks",
                                                                    "FARWingAerodynamicModel",
                                                                  };
        public static readonly Type[] IncompatibleModuleTypes;

        static ModuleB9PartSwitch()
        {
            List<Type> incompatibleTypes = new List<Type>();

            for (int i = 0; i < IncompatibleModuleNames.Length; i++)
            {
                try
                {
                    Type t = Type.GetType(IncompatibleModuleNames[i]);
                    if (t == null)
                        continue;

                    if (!t.IsSubclassOf(typeof(PartModule)))
                    {
                        Debug.LogError("Error: The incompatible type " + IncompatibleModuleNames[i] + " does not derive from PartModule");
                        continue;
                    }
                    incompatibleTypes.Add(t);
                }
                catch(Exception e)
                {
                    Debug.LogError("Exception thrown while getting type " + IncompatibleModuleNames[i] + ": " + e.ToString());
                }
            }

            IncompatibleModuleTypes = incompatibleTypes.ToArray();
        }

        #endregion

        #region Public Fields

        [ConfigNodeSerialized]
        [ConfigField(configName = "SUBTYPE")]
        public List<PartSubtype> subtypes = new List<PartSubtype>();

        [ConfigField]
        public float baseVolume = 0f;

        [ConfigField]
        public string switcherDescription = "Subtype";

        [ConfigField(persistant = true)]
        public int currentSubtypeIndex = 0;

        [ConfigField]
        public bool affectDragCubes = true;

        [ConfigField]
        public bool affectFARVoxels = true;

        // Can't use built-in symmetry because it doesn't know how to find the correct module on the other part
        [KSPField(guiActiveEditor = true, isPersistant = true, guiName = "Subtype")]
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, options = new[] { "None" }, scene = UI_Scene.Editor, suppressEditorShipModified = true)]
        public int subtypeIndexControl = 0;

        #endregion

        #region Private Fields

        [SerializeField]
        private List<string> managedResourceNames = new List<string>();

        [SerializeField]
        private List<string> managedTransformNames = new List<string>();

        [SerializeField]
        private List<string> managedStackNodeIDs = new List<string>();

        #endregion

        #region Properties

        public int SubtypesCount => subtypes.Count;

        public PartSubtype CurrentSubtype => subtypes[currentSubtypeIndex];

        public TankType CurrentTankType => CurrentSubtype.tankType;

        public float TankVolume
        {
            get
            {
                if (CurrentSubtype.HasTank)
                    return baseVolume * CurrentSubtype.volumeMultiplier + CurrentSubtype.volumeAdded;
                else
                    return 0f;
            }
        }

        public PartSubtype this[int index] => subtypes[index];

        public bool UseSmallGUI => SubtypesCount < 4;

        public bool MaxTempManaged { get; private set; }
        public bool SkinMaxTempManaged { get; private set; }
        public bool AttachNodeManaged { get; private set; }

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

            // Initialize stuff

            managedResourceNames = new List<string>();
            managedTransformNames = new List<string>();
            managedStackNodeIDs = new List<string>();

            MaxTempManaged = false;
            SkinMaxTempManaged = false;
            AttachNodeManaged = false;

            for (int i = 0; i < subtypes.Count; i++)
            {
                PartSubtype subtype = subtypes[i];
                subtype.SetParent(this);
                subtype.OnStart();
                TankType tank = subtype.tankType;

                if (tank == null)
                    LogError("Tank is null on subtype " + subtype.Name);

                if (tank.ResourcesCount > 0 && (TankVolumeForSubtype(i) <= 0f))
                {
                    LogError("Subtype " + subtype.Name + " has a tank type with resources, but no volume is specifified");
                    subtype.tankType = tank = B9TankSettings.StructuralTankType;
                }

                if (tank != null)
                {
                    managedResourceNames.AddRange(tank.ResourceNames);
                }

                managedTransformNames.AddRange(subtype.transformNames);
                managedStackNodeIDs.AddRange(subtype.NodeIDs);

                if (subtype.maxTemp > 0f)
                    MaxTempManaged = true;
                if (subtype.skinMaxTemp > 0f)
                    SkinMaxTempManaged = true;
                if (subtype.attachNode.IsNotNull())
                {
                    if (part.attachRules.allowSrfAttach  && part.srfAttachNode != null)
                    {
                        AttachNodeManaged = true;
                    }
                    else
                    {
                        LogError("Error: Part subtype '" + subtype.Name + "' has an attach node defined, but part does not allow surface attachment (or the surface attach node could not be found)");
                    }
                }
            }

            if (currentSubtypeIndex >= subtypes.Count || currentSubtypeIndex < 0)
                currentSubtypeIndex = 0;

            bool editor = (state == StartState.Editor);
            
            SetupGUI();

            for (int i = 0; i < subtypes.Count; i++)
            {
                subtypes[i].DeactivateObjects();
                if (editor)
                    subtypes[i].DeactivateNodes();
                else
                    subtypes[i].ActivateNodes();
            }

            UpdateSubtype(false);
        }

        // This runs after OnStart() so everything should be initalized
        public void Start()
        {
            // Check for incompatible modules
            bool modifiedSetup = false;

            List<ModuleB9PartSwitch> otherModules = part.FindModulesImplementing<ModuleB9PartSwitch>();
            for (int i = 0; i < otherModules.Count; i++)
            {
                ModuleB9PartSwitch otherModule = otherModules[i];
                if (otherModule == this) continue;
                bool destroy = false;
                for (int j = 0; j < managedResourceNames.Count; j++)
                {
                    if (otherModule.IsManagedResource(managedResourceNames[j]))
                    {
                        LogError("Two ModuleB9PartSwitch modules cannot manage the same resource: " + managedResourceNames[j]);
                        destroy = true;
                    }
                }
                for (int j = 0; j < managedTransformNames.Count; j++)
                {
                    if (otherModule.IsManagedTransform(managedTransformNames[j]))
                    {
                        LogError("Two ModuleB9PartSwitch modules cannot manage the same transform: " + managedTransformNames[j]);
                        destroy = true;
                    }
                }
                for (int j = 0; j < managedStackNodeIDs.Count; j++)
                {
                    if (otherModule.IsManagedNode(managedStackNodeIDs[j]))
                    {
                        LogError("Two ModuleB9PartSwitch modules cannot manage the same attach node: " + managedStackNodeIDs[j]);
                        destroy = true;
                    }
                }

                if (otherModule.MaxTempManaged && MaxTempManaged)
                {
                    LogError("Two ModuleB9PartSwitch modules cannot both manage the part's maxTemp");
                    destroy = true;
                }

                if (otherModule.SkinMaxTempManaged && SkinMaxTempManaged)
                {
                    LogError("Two ModuleB9PartSwitch modules cannot both manage the part's skinMaxTemp");
                    destroy = true;
                }

                if (otherModule.AttachNodeManaged && AttachNodeManaged)
                {
                    LogError("Two ModuleB9PartSwitch modules cannot both manage the part's attach node");
                    destroy = true;
                }

                if (destroy)
                {
                    LogWarning("ModuleB9PartSwitch with moduleID '" + otherModule.moduleID + "' is incomatible, and will be removed.");
                    part.Modules.Remove(otherModule);
                    Destroy(otherModule);
                    modifiedSetup = true;
                }
            }

            for (int i = 0; i < part.Modules.Count; i++)
            {
                PartModule m = part.Modules[i];
                if (m == null || m is ModuleB9PartSwitch)
                    continue;
                Type mType = m.GetType();

                for (int j = 0; j < IncompatibleModuleTypes.Length; j++)
                {
                    Type testType = IncompatibleModuleTypes[j];
                    if (mType == testType || mType.IsSubclassOf(testType))
                    {
                        LogError("ModuleB9PartSwitch and " + m.moduleName + " cannot exist on the same part.  " + m.moduleName + " will be removed.");
                        part.Modules.Remove(m);
                        Destroy(m);
                        modifiedSetup = true;
                        break;
                    }
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
            return CurrentSubtype.addedMass + (TankVolume * CurrentTankType.tankMass);
        }

        public ModifierChangeWhen GetModuleMassChangeWhen() => ModifierChangeWhen.FIXED;

        public float GetModuleCost(float baseCost, ModifierStagingSituation situation)
        {
            float cost = CurrentSubtype.addedCost + (TankVolume * CurrentTankType.tankCost);
            cost += TankVolume * CurrentTankType.ResourceUnitCost;
            return cost;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen() => ModifierChangeWhen.FIXED;

        public override string GetInfo()
        {
            string outStr = "<b>" + subtypes.Count.ToString() + " Subtypes:</b>";
            for (int i = 0; i < subtypes.Count; i++)
            {
                outStr += "\n  <b>- " + subtypes[i].title + "</b>";
                int resourceCount = subtypes[i].tankType.ResourcesCount;
                if (resourceCount > 0)
                {
                    outStr += "\n      <b><color=#99ff00ff>Resources:</color></b>";
                    float volume = TankVolumeForSubtype(i);
                    for (int j = 0; j < resourceCount; j++)
                        outStr += "\n      <b>- " + subtypes[i].tankType.resources[j].ResourceName + "</b>: " + (subtypes[i].tankType.resources[j].unitsPerVolume * volume).ToString("F1");
                }
            }
            return outStr;
        }

        public string GetModuleTitle()
        {
            return "Switchable Part";
        }

        public string GetPrimaryField()
        {
            string outStr = "<b>" + subtypes.Count.ToString() + " Subtypes</b>";
            if (baseVolume > 0)
                outStr += "\n  <b>Volume:</b> " + baseVolume.ToString("F0");
            return outStr;
        }

        public Callback<Rect> GetDrawModulePanelCallback()
        {
            return null;
        }

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

        public float TankVolumeForSubtype(int index)
        {
            if (index < 0 || index >= SubtypesCount)
                throw new IndexOutOfRangeException("Index " + index.ToString() + " is out of range (there are " + SubtypesCount.ToString() + "subtypes.");
            PartSubtype subtype = subtypes[index];
            if (subtype == null || subtype.tankType == null || subtype.tankType.ResourcesCount == 0)
                return 0f;
            else
                return baseVolume * subtype.volumeMultiplier + subtype.volumeAdded;
        }

        #endregion

        #region Private Methods

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
            SetNewSubtype(subtypeIndexControl, false);
        }

        private void SetNewSubtype(int newIndex, bool force)
        {
            // For symmetry
            subtypeIndexControl = newIndex;

            if (newIndex == currentSubtypeIndex && !force)
                return;

            if (newIndex < 0 || newIndex >= subtypes.Count)
                throw new ArgumentException("Subtype index must be between 0 and " + subtypes.Count.ToString());

            if (newIndex != currentSubtypeIndex)
            {
                CurrentSubtype.DeactivateObjects();
                if (HighLogic.LoadedSceneIsEditor)
                    CurrentSubtype.DeactivateNodes();
            }

            currentSubtypeIndex = newIndex;

            UpdateSubtype(true);

            foreach (var counterpart in this.FindSymmetryCounterparts<ModuleB9PartSwitch>())
                counterpart.SetNewSubtype(newIndex, force);
        }

        private void UpdateSubtype(bool fillTanks)
        {
            CurrentSubtype.ActivateObjects();
            if (HighLogic.LoadedSceneIsEditor)
                CurrentSubtype.ActivateNodes();

            UpdateTankSetup(fillTanks);

            if (CurrentSubtype.maxTemp > 0)
                part.maxTemp = CurrentSubtype.maxTemp;
            else
                part.maxTemp = part.GetPrefab().maxTemp;

            if (CurrentSubtype.skinMaxTemp > 0)
                part.skinMaxTemp = CurrentSubtype.skinMaxTemp;
            else
                part.skinMaxTemp = part.GetPrefab().skinMaxTemp;

            if (AttachNodeManaged && part.attachRules.allowSrfAttach && part.srfAttachNode != null)
            {
                var referenceNode = CurrentSubtype.attachNode ?? part.GetPrefab().srfAttachNode;
                part.srfAttachNode.position = referenceNode.position;
                part.srfAttachNode.orientation = referenceNode.orientation;
                part.srfAttachNode.size = referenceNode.size;
            }

            if (FARWrapper.FARLoaded && affectFARVoxels && managedTransformNames.Count > 0)
            {
                part.SendMessage("GeometryPartModuleRebuildMeshData");
            }
            
            if (affectDragCubes && managedTransformNames.Count > 0)
            {
                DragCube newCube = DragCubeSystem.Instance.RenderProceduralDragCube(part);
                part.DragCubes.ClearCubes();
                part.DragCubes.Cubes.Add(newCube);
                part.DragCubes.ResetCubeWeights();
            }

            var window = FindObjectsOfType<UIPartActionWindow>().FirstOrDefault(w => w.part == part);
            if (window.IsNotNull())
                window.displayDirty = true;

            if (HighLogic.LoadedSceneIsEditor)
            {
                GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartTweaked, part);
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                GameEvents.onVesselWasModified.Fire(this.vessel);
            }

            LogInfo("Switched subtype to " + CurrentSubtype.Name);
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
                float resourceAmount = resource.unitsPerVolume * TankVolume;
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

        #endregion
    }
}
