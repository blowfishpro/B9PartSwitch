using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UniLinq;
using UnityEngine;
using B9PartSwitch.Fishbones;

namespace B9PartSwitch
{
    public interface ILinearScaleProvider
    {
        float LinearScale { get; }
    }

    public class ModuleB9PartSwitch : CustomPartModule, IPartMassModifier, IPartCostModifier, IModuleInfo, ILinearScaleProvider
    {
        #region Constants

        private static readonly string[] INCOMAPTIBLE_MODULES_FOR_RESOURCE_SWITCHING = { "FSfuelSwitch", "InterstellarFuelSwitch", "ModuleFuelTanks" };

        #endregion

        #region Node Data Fields

        [NodeData(name = "SUBTYPE", alwaysSerialize = true)]
        public List<PartSubtype> subtypes = new List<PartSubtype>();

        [NodeData]
        public float baseVolume = 0f;

        [NodeData]
        public string switcherDescription = Localization.ModuleB9PartSwitch_DefaultSwitcherDescription; // Subtype

        [NodeData]
        public string switcherDescriptionPlural = Localization.ModuleB9PartSwitch_DefaultSwitcherDescriptionPlural; // Subtypes

        [NodeData]
        public bool affectDragCubes = true;

        [NodeData]
        public bool affectFARVoxels = true;

        [NodeData]
        public string parentID = null;

        [NodeData]
        public bool switchInFlight = false;

        [NodeData]
        public bool advancedTweakablesOnly = false;

        [NodeData(name = "currentSubtype", persistent = true)]
        public string CurrentSubtypeName
        {
            get => subtypes.Count > 0 ? CurrentSubtype?.Name : null;
            private set
            {
                int index = subtypes.FindIndex(subtype => subtype.Name == value);
                if (index == -1) LogError($"Cannot assign subtype because no subtype with name = '{value}' exists");
                else currentSubtypeIndex = index;
            }
        }

        // Can't use built-in symmetry because it doesn't know how to find the correct module on the other part
        [KSPField(guiActiveEditor = true, guiName = "Subtype")]
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.None, scene = UI_Scene.Editor, suppressEditorShipModified = true)]
        public int currentSubtypeIndex = -1;

        [KSPField]
        public string currentSubtypeTitle = null;

        #endregion

        #region Events

        [KSPEvent(guiActiveEditor = true)]
        public void ShowSubtypesWindow() => PartSwitchFlightDialog.Spawn(this);

        #endregion

        #region Private Fields

        // Tweakscale integration (set via reflection, readonly is ok)
        [SuppressMessage("Style", "IDE0044:Add readonly modifier")]
        private readonly float scale = 1f;

        private ModuleB9PartSwitch parent;
        private List<ModuleB9PartSwitch> children = new List<ModuleB9PartSwitch>(0);

        #endregion

        #region Properties

        public int SubtypesCount => subtypes.Count;

        // Provide a default of zero in case best subtype has not yet been determined
        public int SubtypeIndex => subtypes.ValidIndex(currentSubtypeIndex) ? currentSubtypeIndex : 0;
        public PartSubtype CurrentSubtype => subtypes[SubtypeIndex];

        public IEnumerable<PartSubtype> InactiveSubtypes => subtypes.Where(subtype => subtype != CurrentSubtype);

        public TankType CurrentTankType => CurrentSubtype.tankType;

        public float VolumeFromChildren { get; private set; } = 0f;
        public float VolumeAddedToParent => CurrentSubtype.volumeAddedToParent;

        public PartSubtype this[int index] => subtypes[index];

        public IEnumerable<Transform> ManagedTransforms => subtypes.SelectMany(subtype => subtype.Transforms);
        public IEnumerable<AttachNode> ManagedNodes => subtypes.SelectMany(subtype => subtype.Nodes);
        public IEnumerable<string> ManagedResourceNames => subtypes.SelectMany(subtype => subtype.ResourceNames);
        public IEnumerable<Material> ManagedMaterials => subtypes.SelectMany(subtype => subtype.Materials);
        public IEnumerable<AttachNode> AttachNodesWithManagedPosition => subtypes.SelectMany(subtype => subtype.AttachNodesWithManagedPosition);

        public bool ManagesTransforms => ManagedTransforms.Any();
        public bool ManagesNodes => ManagedNodes.Any();
        public bool ManagesResources => subtypes.Any(s => !s.tankType.IsStructuralTankType);
        public bool ChangesMass => subtypes.Any(s => s.ChangesMass);
        public bool ChangesCost => subtypes.Any(s => s.ChangesCost);

        public float Scale => scale;
        public float LinearScale => scale;
        public float VolumeScale => scale * scale * scale;

        #endregion

        #region Setup

        public override void OnAwake()
        {
            base.OnAwake();

            InitializeSubtypes();
        }

        protected override void OnLoadPrefab(ConfigNode node)
        {
            base.OnLoadPrefab(node);

            if (subtypes.Count == 0)
            {
                Exception ex = new Exception($"No subtypes found on {this}");
                FatalErrorHandler.HandleFatalError(ex);
                throw ex;
            }

            string[] duplicatedNames = subtypes.GroupBy(s => s.Name).Where(g => g.Count() > 1).Select(g => g.Key).ToArray();

            if (duplicatedNames.Length > 0)
            {
                string duplicatedNamesString = string.Join(", ", duplicatedNames);
                SeriousWarningHandler.DisplaySeriousWarning($"Duplicated subtype names found on {this}: {duplicatedNamesString}");
                LogError($"Duplicate subtype names detected: {duplicatedNamesString}");
            }

            InitializeSubtypes();
        }

        protected override void OnLoadInstance(ConfigNode node)
        {
            base.OnLoadInstance(node);

            InitializeSubtypes();
            FindBestSubtype(node);
            UpdateOnLoad();
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

            IEnumerator InvokeUpdateAfterStart()
            {
                yield return null;
                UpdateAfterStart();
            }

            StartCoroutine(InvokeUpdateAfterStart());
        }

        // This runs after OnStart() so everything should be initalized
        public void Start()
        {
            CheckOtherSwitchers();
            CheckOtherModules();

            if (affectDragCubes) part.FixModuleJettison();

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
                    outStr += $"\n  <color=#99ff00ff>- {resource.resourceDefinition.displayName}</color>: {resource.unitsPerVolume * subtype.TotalVolume :F1}";
            }
            return outStr;
        }

        public string GetModuleTitle() => Localization.ModuleB9PartSwitch_ModuleTitle; // Switchable Part

        public string GetPrimaryField()
        {
            string outStr = $"<b>{subtypes.Count} {switcherDescriptionPlural}</b>";
            if (baseVolume > 0)
                outStr += $" (<b>{Localization.ModuleB9PartSwitch_TankVolumeString}:</b> {baseVolume :F0})"; // Volume
            return outStr;
        }

        public Callback<Rect> GetDrawModulePanelCallback() => null;

        #endregion

        #region Callbacks

        private void OnSliderUpdate(BaseField field, object oldFieldValueObj)
        {
            int oldIndex = (int)oldFieldValueObj;

            subtypes[oldIndex].DeactivateOnSwitch();

            UpdateOnSwitch();
        }

        #endregion

        #region Public Methods

        public void SwitchSubtype(string name)
        {
            CurrentSubtype.DeactivateOnSwitch();
            CurrentSubtypeName = name;

            UpdateOnSwitch();
        }

        public bool IsManagedResource(string resourceName) => ManagedResourceNames.Contains(resourceName);

        public bool TransformShouldBeEnabled(Transform transform)
        {
            if (CurrentSubtype.TransformIsManaged(transform)) return true;
            foreach (PartSubtype subtype in subtypes)
            {
                if (subtype == CurrentSubtype) continue;
                if (subtype.TransformIsManaged(transform)) return false;
            }

            return true;
        }

        public bool NodeShouldBeEnabled(AttachNode node)
        {
            if (CurrentSubtype.NodeManaged(node)) return true;
            foreach (PartSubtype subtype in subtypes)
            {
                if (subtype == CurrentSubtype) continue;
                if (subtype.NodeManaged(node)) return false;
            }

            return true;
        }

        public bool IsManagedMaterial(Material material) => ManagedMaterials.Contains(material);

        public bool IsAttachNodePositionManaged(AttachNode attachNode) => AttachNodesWithManagedPosition.Contains(attachNode);

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
            CurrentSubtype.UpdateVolume();
        }

        public override void OnWillBeCopied(bool asSymCounterpart)
        {
            base.OnWillBeCopied(asSymCounterpart);

            foreach (PartSubtype subtype in InactiveSubtypes)
            {
                subtype.OnWillBeCopiedInactiveSubtype();
            }

            CurrentSubtype.OnWillBeCopiedActiveSubtype();
        }

        public override void OnWasCopied(PartModule copyPartModule, bool asSymCounterpart)
        {
            base.OnWasCopied(copyPartModule, asSymCounterpart);

            foreach (PartSubtype subtype in InactiveSubtypes)
            {
                subtype.OnWasCopiedInactiveSubtype();
            }

            CurrentSubtype.OnWasCopiedActiveSubtype();
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
            foreach (PartSubtype subtype in subtypes)
            {
                if (subtype == CurrentSubtype) continue;
                subtype.DeactivateForIcon();
            }

            CurrentSubtype.ActivateForIcon();
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

        private void FindBestSubtype(ConfigNode node = null)
        {
            if (node?.GetValue("currentSubtypeName") is string name)
            {
                CurrentSubtypeName = name;
            }

            if (subtypes.ValidIndex(currentSubtypeIndex)) return;

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
        }

        private void SetupGUI()
        {
            BaseField chooseField = Fields[nameof(currentSubtypeIndex)];
            chooseField.guiName = switcherDescription;
            chooseField.advancedTweakable = advancedTweakablesOnly;
            chooseField.guiActiveEditor = subtypes.Count > 1;

            UI_ChooseOption chooseOption = (UI_ChooseOption)chooseField.uiControlEditor;
            chooseOption.options = subtypes.Select(s => s.title).ToArray();
            chooseOption.onFieldChanged = OnSliderUpdate;

            BaseEvent switchSubtypeEvent = Events[nameof(ShowSubtypesWindow)];
            switchSubtypeEvent.guiName = $"Switch {switcherDescription}";
            switchSubtypeEvent.advancedTweakable = advancedTweakablesOnly;
            switchSubtypeEvent.guiActiveEditor = subtypes.Count > 1;

            BaseField subtypeTitleField = Fields[nameof(currentSubtypeTitle)];
            subtypeTitleField.guiName = switcherDescription;
            subtypeTitleField.advancedTweakable = advancedTweakablesOnly;
            subtypeTitleField.guiActiveEditor = subtypes.Count == 1;

            if (HighLogic.LoadedSceneIsFlight)
                UpdateSwitchEventFlightVisibility();
        }

        private void UpdateSwitchEventFlightVisibility()
        {
            bool switchInFlightEnabled = subtypes.Any(s => s != CurrentSubtype && s.allowSwitchInFlight);
            BaseEvent switchSubtypeEvent = Events[nameof(ShowSubtypesWindow)];
            switchSubtypeEvent.guiActive = switchInFlight && switchInFlightEnabled;

            BaseField subtypeTitleField = Fields[nameof(currentSubtypeTitle)];
            subtypeTitleField.guiActive = switchInFlight && !switchInFlightEnabled;
        }

        private void UpdateOnLoad()
        {
            subtypes.ForEach(subtype => subtype.DeactivateOnStart());
            RemoveUnusedResources();
            UpdateVolumeFromChildren();
            CurrentSubtype.ActivateOnStart();
        }

        private void UpdateOnStart()
        {
            subtypes.ForEach(subtype => subtype.DeactivateOnStart());
            RemoveUnusedResources();
            UpdateVolumeFromChildren();
            CurrentSubtype.ActivateOnStart();
            UpdateGeometry(true);
            currentSubtypeTitle = CurrentSubtype.title;

            LogInfo($"Switched subtype to {CurrentSubtype.Name}");
        }

        private void UpdateAfterStart()
        {
            CurrentSubtype.ActivateAfterStart();
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
                string error = "";
                foreach (string resourceName in ManagedResourceNames)
                {
                    if (otherModule.IsManagedResource(resourceName))
                    {
                        error += $"\n  Two modules cannot manage the same resource: {resourceName}";
                    }
                }

                foreach (Material material in ManagedMaterials)
                {
                    if (otherModule.IsManagedMaterial(material))
                    {
                        error += $"\n  Two modules cannot manage the same material: {material.name}";
                    }
                }

                foreach (AttachNode attachNode in AttachNodesWithManagedPosition)
                {
                    if (otherModule.IsAttachNodePositionManaged(attachNode))
                    {
                        error += $"\n  Two modules cannot manage the same attach node's postition: {attachNode.id}";
                    }
                }

                foreach (ISubtypePartField field in SubtypePartFields.All)
                {
                    if (PartFieldManaged(field) && otherModule.PartFieldManaged(field))
                    {
                        error += $"\n  Two modules cannot both manage the part's {field.Name}";
                    }
                }

                if (error != "")
                {
                    Exception ex = new Exception($"Conflict found between {this} and {otherModule}:" + error);
                    FatalErrorHandler.HandleFatalError(ex);
                    throw ex;
                }
            }
        }

        private void CheckOtherModules()
        {
            if (ManagesResources)
            {
                string[] incompatibleModulesOnPart = INCOMAPTIBLE_MODULES_FOR_RESOURCE_SWITCHING.Where(modName => part.Modules.Contains(modName)).ToArray();

                if (incompatibleModulesOnPart.Length > 0)
                {
                    foreach (PartSubtype subtype in subtypes)
                    {
                        subtype.AssignStructuralTankType();
                    }
                    SeriousWarningHandler.DisplaySeriousWarning($"{this} and {string.Join(", ", incompatibleModulesOnPart)} - cannot both manage resources on the same part, B9 resource switching will be disabled");
                }

            }
        }

        #endregion

        private void UpdateOnSwitch()
        {
            UpdateSubtype();

            if (HighLogic.LoadedSceneIsEditor)
            {
                foreach (var counterpart in this.FindSymmetryCounterparts())
                    counterpart.UpdateFromSymmetry(currentSubtypeIndex);

                GameEvents.onEditorPartEvent.Fire(ConstructionEventType.PartTweaked, part);
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);
            }
            else if (HighLogic.LoadedSceneIsFlight)
            {
                GameEvents.onVesselWasModified.Fire(this.vessel);

                UpdateSwitchEventFlightVisibility();
            }

            UpdatePartActionWindow();
        }

        private void UpdateFromSymmetry(int newIndex)
        {
            CurrentSubtype.DeactivateOnSwitch();

            currentSubtypeIndex = newIndex;

            UpdateSubtype();
        }

        private void UpdateSubtype()
        {
            CurrentSubtype.ActivateOnSwitch();
            UpdateGeometry(false);
            parent?.UpdateVolume();
            currentSubtypeTitle = CurrentSubtype.title;
            LogInfo($"Switched subtype to {CurrentSubtype.Name}");
        }

        private void UpdateDragCubesOnAttach()
        {
            IEnumerator UpdateDragCubesOnAttachCoroutine()
            {
                yield return null;
                RenderProceduralDragCubes();
            }

            part.OnEditorAttach -= UpdateDragCubesOnAttach;
            StartCoroutine(UpdateDragCubesOnAttachCoroutine());
        }

        private void UpdateDragCubesForRootPartInFlight()
        {
            IEnumerator UpdateDragCubesForRootPartInFlightCoroutine()
            {
                yield return null;
                yield return null;
                yield return null;

                RenderProceduralDragCubes();
            }

            StartCoroutine(UpdateDragCubesForRootPartInFlightCoroutine());
        }

        private void UpdateGeometry(bool start)
        {
            if (!ManagesTransforms) return;

            if (FARWrapper.FARLoaded && affectFARVoxels)
            {
                part.SendMessage("GeometryPartModuleRebuildMeshData");
            }

            if (affectDragCubes && (!start || IsLastModuleAffectingDragCubes()))
            {
                if (HighLogic.LoadedSceneIsEditor && part.parent == null && EditorLogic.RootPart != part)
                    part.OnEditorAttach += UpdateDragCubesOnAttach;
                else if (HighLogic.LoadedSceneIsFlight && part.parent == null)
                    UpdateDragCubesForRootPartInFlight();
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

        private bool IsLastModuleAffectingDragCubes()
        {
            ModuleB9PartSwitch lastModule = part.Modules.OfType<ModuleB9PartSwitch>().Where(m => m.ManagesTransforms && m.affectDragCubes).LastOrDefault();
            return ReferenceEquals(this, lastModule);
        }

        private void RenderProceduralDragCubes()
        {
            part.DragCubes.ClearCubes();
            StartCoroutine(DragCubeSystem.Instance.SetupDragCubeCoroutine(part, null));
        }

        private void UpdateVolumeFromChildren()
        {
            VolumeFromChildren = children.Sum(child => child.VolumeAddedToParent);
        }

        #endregion
    }
}
