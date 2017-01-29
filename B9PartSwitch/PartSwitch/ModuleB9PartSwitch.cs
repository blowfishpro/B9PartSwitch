using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using B9PartSwitch.Fishbones;

namespace B9PartSwitch
{
    public class ModuleB9PartSwitch : CustomPartModule, IPartMassModifier, IPartCostModifier, IModuleInfo
    {
        #region Public Fields
        
        [NodeData(name = "SUBTYPE", alwaysSerialize = true)]
        public List<PartSubtype> subtypes = new List<PartSubtype>();

        [NodeData]
        public float baseVolume = 0f;

        [NodeData]
        public string switcherDescription = "Subtype";

        [NodeData]
        public string switcherDescriptionPlural = "Subtypes";

        [NodeData]
        public bool affectDragCubes = true;

        [NodeData]
        public bool affectFARVoxels = true;

        [NodeData]
        public string parentID = null;

        [NodeData(persistent = true)]
        public string currentSubtypeName = null;

        // Can't use built-in symmetry because it doesn't know how to find the correct module on the other part
        [KSPField(guiActiveEditor = true, isPersistant = true, guiName = "Subtype")]
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, scene = UI_Scene.Editor, suppressEditorShipModified = true)]
        public int currentSubtypeIndex = -1;

        #endregion

        #region Private Fields

        // Tweakscale integration
        private float scale = 1f;

        private ModuleB9PartSwitch parent;
        private List<ModuleB9PartSwitch> children = new List<ModuleB9PartSwitch>(0);

        #endregion

        #region Properties

        public int SubtypesCount => subtypes.Count;

        // Provide a default of zero in case best subtype has not yet been determined
        public int SubtypeIndex => subtypes.ValidIndex(currentSubtypeIndex) ? currentSubtypeIndex : 0;
        public PartSubtype CurrentSubtype => subtypes[SubtypeIndex];

        public TankType CurrentTankType => CurrentSubtype.tankType;

        public float CurrentVolume => CurrentSubtype.TotalVolume * VolumeScale;
        public float VolumeFromChildren { get; private set; } = 0f;
        public float ParentVolume => CurrentSubtype.volumeAddedToParent;

        public PartSubtype this[int index] => subtypes[index];

        public IEnumerable<Transform> ManagedTransforms => subtypes.SelectMany(subtype => subtype.Transforms);
        public IEnumerable<AttachNode> ManagedNodes => subtypes.SelectMany(subtype => subtype.Nodes);
        public IEnumerable<string> ManagedResourceNames => subtypes.SelectMany(subtype => subtype.ResourceNames);

        public bool ManagesTransforms => ManagedTransforms.Any();
        public bool ManagesNodes => ManagedNodes.Any();
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

            InitializeSubtypes();
        }

        public override void OnIconCreate()
        {
            base.OnIconCreate();

            InitializeSubtypes();
            SetupForIcon();
        }

        public override void OnStart(PartModule.StartState state)
        {
            base.OnStart(state);

            FindParent();

            InitializeSubtypes();
            SetupSubtypes();

            FindBestSubtype();

            SetupGUI();
        }

        // This runs after OnStart() so everything should be initalized
        public void Start()
        {
            CheckOtherSwitchers();
            CheckOtherModules();
            
            UpdateOnStart();
        }

        #endregion

        #region Interface Methods

        public float GetModuleMass(float baseMass, ModifierStagingSituation situation) => CurrentSubtype.TotalMass * VolumeScale;

        public ModifierChangeWhen GetModuleMassChangeWhen() => ModifierChangeWhen.FIXED;

        public float GetModuleCost(float baseCost, ModifierStagingSituation situation) => CurrentSubtype.TotalCost * VolumeScale;

        public ModifierChangeWhen GetModuleCostChangeWhen() => ModifierChangeWhen.FIXED;

        public override string GetInfo()
        {
            string outStr = $"<b><color=#7fdfffff>{SubtypesCount} {switcherDescriptionPlural}</color></b>";
            foreach (var subtype in subtypes)
            {
                outStr += $"\n<b>- {subtype.title}</b>";
                foreach (var resource in subtype.tankType)
                    outStr += $"\n  <color=#99ff00ff>- {resource.ResourceName}</color>: {resource.unitsPerVolume * subtype.TotalVolume :F1}";
            }
            return outStr;
        }

        public string GetModuleTitle() => "Switchable Part";

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

        public bool IsManagedResource(string resourceName) => ManagedResourceNames.Contains(resourceName);
        public bool IsManagedTransform(Transform transform) => ManagedTransforms.Contains(transform);
        public bool IsManagedNode(AttachNode node) => ManagedNodes.Contains(node);

        public bool PartFieldManaged(ISubtypePartField field) => subtypes.Any(subtype => field.ShouldUseOnSubtype(subtype.Context));

        public void AddChild(ModuleB9PartSwitch child)
        {
            child.ThrowIfNullArgument(nameof(child));

            if (children.Contains(child))
            {
                LogError($"Child module with id '{child.moduleID}' has already been added!");
                return;
            }

            children.Add(child);
        }

        public void UpdateVolume()
        {
            UpdateVolumeFromChildren();
            CurrentSubtype.AddResources(true);
        }

        #endregion

        #region Private Methods

        #region Setup

        private void FindParent()
        {
            parent = null;
            if (parentID.IsNullOrEmpty()) return;

            parent = part.Modules.OfType<ModuleB9PartSwitch>().FirstOrDefault(module => module.moduleID == parentID);

            if (parent.IsNull())
            {
                LogError($"Cannot find parent module with id '{parentID}'");
                return;
            }

            parent.AddChild(this);
        }

        private void InitializeSubtypes()
        {
            foreach (PartSubtype subtype in subtypes)
            {
                subtype.Setup(this);
            }
        }

        private void SetupForIcon()
        {
            // This will deactivate objects on non-active subtypes before the part icon is created, avoiding a visual mess
            foreach (var subtype in subtypes)
            {
                subtype.DeactivateObjects();
            }
            CurrentSubtype.ActivateObjects();
        }

        private void SetupSubtypes()
        {
            foreach (var subtype in subtypes)
            {
                if (subtype.tankType == null)
                    LogError($"Tank is null on subtype {subtype.Name}");

                if (subtype.tankType.ResourcesCount > 0 && (subtype.TotalVolume <= 0f))
                {
                    LogError($"Subtype {subtype.Name} has a tank type with resources, but no volume is specifified");
                    subtype.AssignStructuralTankType();
                }
            }

            if (PartFieldManaged(SubtypePartFields.SrfAttachNode) && !part.attachRules.allowSrfAttach || part.srfAttachNode.IsNull())
            {
                LogError($"Error: One or more subtypes have an attach node defined, but part does not allow surface attachment (or the surface attach node could not be found)");
                subtypes.ForEach(subtype => subtype.ClearAttachNode());
            }
        }

        private void FindBestSubtype()
        {
            // First try to identify subtype by name
            if (!currentSubtypeName.IsNullOrEmpty())
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
                string[] resourcesOnPart = ManagedResourceNames.Intersect(part.Resources.Select(resource => resource.resourceName)).ToArray();

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
            UpdateVolumeFromChildren();
            CurrentSubtype.ActivateOnStart();
            UpdateGeometry();

            LogInfo($"Switched subtype to {CurrentSubtype.Name}");
        }

        private void RemoveUnusedResources()
        {
            for (int i = part.Resources.Count - 1; i >= 0 ; i --)
            {
                PartResource resource = part.Resources[i];
                if (IsManagedResource(resource.resourceName) && !CurrentTankType.ContainsResource(resource.resourceName))
                {
                    part.Resources.Remove(resource);
                }
            }
        }

        private void CheckOtherSwitchers()
        {
            foreach (var otherModule in part.Modules.OfType<ModuleB9PartSwitch>())
            {
                if (otherModule == this) continue;
                bool destroy = false;
                foreach (string resourceName in ManagedResourceNames)
                {
                    if (otherModule.IsManagedResource(resourceName))
                    {
                        LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot manage the same resource: {resourceName}");
                        destroy = true;
                    }
                }
                foreach (Transform transform in ManagedTransforms)
                {
                    if (otherModule.IsManagedTransform(transform))
                    {
                        LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot manage the same transform: {transform.name}");
                        destroy = true;
                    }
                }
                foreach (AttachNode node in ManagedNodes)
                {
                    if (otherModule.IsManagedNode(node))
                    {
                        LogError($"Two {nameof(ModuleB9PartSwitch)} modules cannot manage the same attach node: {node.id}");
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
                }
            }
        }

        private void CheckOtherModules()
        {
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
                            subtype.AssignStructuralTankType();
                    }
                }

            }
        }

        #endregion

        private void UpdateFromGUI(BaseField field, object oldFieldValueObj)
        {
            int oldIndex = (int)oldFieldValueObj;

            subtypes[oldIndex].DeactivateOnSwitch();

            UpdateSubtype();

            foreach (var counterpart in this.FindSymmetryCounterparts())
                counterpart.UpdateFromSymmetry(currentSubtypeIndex);

            UpdatePartActionWindow();
            FireEvents();
        }

        private void UpdateFromSymmetry(int newIndex)
        {
            CurrentSubtype.DeactivateOnSwitch();

            currentSubtypeIndex = newIndex;

            UpdateSubtype();
        }

        private void UpdateSubtype()
        {
            currentSubtypeName = CurrentSubtype.Name;

            CurrentSubtype.ActivateOnSwitch();
            UpdateGeometry();
            parent?.UpdateVolume();
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
            if (!ManagesTransforms) return;

            if (FARWrapper.FARLoaded && affectFARVoxels)
            {
                part.SendMessage("GeometryPartModuleRebuildMeshData");
            }

            if (affectDragCubes)
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

        private void UpdateVolumeFromChildren()
        {
            VolumeFromChildren = children.Sum(child => child.ParentVolume);
        }

        #endregion
    }
}
