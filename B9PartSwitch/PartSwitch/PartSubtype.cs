using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UniLinq;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;
using B9PartSwitch.PartSwitch.PartModifiers;

namespace B9PartSwitch
{
    public class PartSubtype : IContextualNode
    {
        #region Config Fields

        [NodeData(name = "name")]
        public string subtypeName;

        [NodeData]
        public string title;

        [NodeData]
        public string descriptionSummary;

        [NodeData]
        public string descriptionDetail;

        [NodeData]
        public Color? primaryColor;

        [NodeData]
        public Color? secondaryColor;

        [NodeData]
        public string upgradeRequired;

        [NodeData]
        public float defaultSubtypePriority = 0;

        [NodeData(name = "transform")]
        public List<string> transformNames = new List<string>();

        [NodeData(name = "node")]
        public List<string> nodeNames = new List<string>();

        [NodeData(name = "TEXTURE")]
        public List<TextureSwitchInfo> textureSwitches = new List<TextureSwitchInfo>();

        [NodeData(name = "NODE")]
        public List<AttachNodeModifierInfo> attachNodeModifierInfos = new List<AttachNodeModifierInfo>();

        [NodeData(name = "TRANSFORM")]
        public List<TransformModifierInfo> transformModifierInfos = new List<TransformModifierInfo>();

        [NodeData(name = "MODULE")]
        public List<ModuleModifierInfo> moduleModifierInfos = new List<ModuleModifierInfo>();

        [NodeData]
        public float addedMass = 0f;

        [NodeData]
        public float addedCost = 0f;

        [UseParser(typeof(TankTypeValueParser))]
        [NodeData]
        public TankType tankType;

        [NodeData]
        public float volumeMultiplier = 1f;

        [NodeData]
        public float volumeAdded = 0f;

        [NodeData]
        public float volumeAddedToParent = 0f;

        [NodeData]
        public float? percentFilled;

        [NodeData]
        public bool? resourcesTweakable;

        [NodeData]
        public float maxTemp;

        [NodeData]
        public float skinMaxTemp;

        [NodeData]
        public AttachNode attachNode = null;

        [NodeData]
        public float crashTolerance = 0f;

        [NodeData]
        public Vector3 CoMOffset = Vector3Extensions.NaN();

        [NodeData]
        public Vector3 CoPOffset = Vector3Extensions.NaN();

        [NodeData]
        public Vector3 CoLOffset = Vector3Extensions.NaN();

        [NodeData]
        public Vector3 CenterOfBuoyancy = Vector3Extensions.NaN();

        [NodeData]
        public Vector3 CenterOfDisplacement = Vector3Extensions.NaN();

        [NodeData]
        public int stackSymmetry = -1;

        [NodeData]
        public bool allowSwitchInFlight = true;

        [NodeData]
        public string mirrorSymmetrySubtype;

        #endregion

        #region Private Fields

        private ModuleB9PartSwitch parent;
        private readonly List<Transform> transforms = new List<Transform>();
        private readonly List<AttachNode> nodes = new List<AttachNode>();
        private readonly List<IPartModifier> partModifiers = new List<IPartModifier>();
        private readonly List<object> aspectLocks = new List<object>();

        #endregion

        #region Properties

        public string Name => subtypeName;

        public bool HasTank => tankType != null && tankType.ResourcesCount > 0;

        public bool HasUpgradeRequired => !upgradeRequired.IsNullOrEmpty();

        public IEnumerable<Transform> Transforms => transforms.Select(transform => transform.transform);
        public IEnumerable<AttachNode> Nodes => nodes.All();
        public IEnumerable<string> ResourceNames => tankType.ResourceNames;
        public IEnumerable<string> NodeIDs => nodes.Select(n => n.id);

        public bool ChangesDryMass => addedMass != 0 || tankType.tankMass != 0;
        public bool ChangesMass => (addedMass != 0f) || tankType.ChangesMass;
        public bool ChangesDryCost => addedCost != 0 || tankType.tankCost != 0;
        public bool ChangesCost => (addedCost != 0f) || tankType.ChangesCost;

        public IEnumerable<object> PartAspectLocks => aspectLocks.All();

        public Color PrimaryColor => primaryColor ?? tankType.primaryColor ?? Color.white;
        public Color SecondaryColor => secondaryColor ?? tankType.secondaryColor ?? primaryColor ?? tankType.primaryColor ?? Color.gray;

        #endregion

        #region Interface Methods

        public void Load(ConfigNode node, OperationContext context)
        {
            OperationContext newContext;

            try
            {
                newContext = this.LoadFields(node, context);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception while loading fields on subtype {this}", ex);
            }

            OnLoad(node, newContext);
        }

        public void Save(ConfigNode node, OperationContext context)
        {
            try
            {
                this.SaveFields(node, context);
            }
            catch (Exception ex)
            {
                throw new Exception($"Exception while loading fields on subtype {this}", ex);
            }
        }

        #endregion

        #region Setup

        private void OnLoad(ConfigNode node, OperationContext context)
        {
            if (Name.IsNullOrEmpty())
            {
                SeriousWarningHandler.DisplaySeriousWarning($"Subtype has no name: {this}");
                LogError("Subtype has no name");
            }

            if (HasUpgradeRequired && PartUpgradeManager.Handler.GetUpgrade(upgradeRequired).IsNull())
            {
                SeriousWarningHandler.DisplaySeriousWarning($"Upgrade does not exist: {upgradeRequired} on: {this}");
                LogError("Upgrade does not exist: " + upgradeRequired);
                upgradeRequired = null;
            }

            if (tankType == null)
                tankType = B9TankSettings.StructuralTankType;

            if (mirrorSymmetrySubtype == null)
                mirrorSymmetrySubtype = Name;

            if (context.Operation == Operation.LoadPrefab)
            {
                if (title.IsNullOrEmpty())
                    title = subtypeName;

                ConfigNode[] resourceNodes = node.GetNodes("RESOURCE");

                if (resourceNodes.Length > 0)
                {
                    LoadAdditionalResources(resourceNodes, context);
                }
            }
        }

        public void OnBeforeReinitializeInactiveSubtype()
        {
            foreach(IPartModifier modifier in partModifiers)
            {
                modifier.OnBeforeReinitializeInactiveSubtype();
            }
        }

        public void OnBeforeReinitializeActiveSubtype()
        {
            foreach (IPartModifier modifier in partModifiers)
            {
                modifier.OnBeforeReinitializeActiveSubtype();
            }
        }

        public void Setup(ModuleB9PartSwitch parent, bool displayWarnings = true)
        {
            if (parent == null)
                throw new ArgumentNullException("parent cannot be null");
            if (parent.part == null)
                throw new ArgumentNullException("parent.part cannot be null");

            this.parent = parent;

            aspectLocks.Clear();

            Part part = parent.part;
            Part partPrefab = part.GetPrefab() ?? part;
            partModifiers.Clear();

            IEnumerable<object> aspectLocksOnOtherModules = parent.PartAspectLocksOnOtherModules;

            string errorString = null;

            void OnInitializationError(string message)
            {
                LogError(message);

                if (displayWarnings)
                {
                    if (errorString == null) errorString = $"Initialization errors on {parent} subtype '{Name}'";

                    errorString += "\n  " + message;
                }
            }

            void MaybeAddModifier(IPartModifier modifier)
            {
                if (modifier == null) return;
                if (aspectLocksOnOtherModules.Contains(modifier.PartAspectLock))
                {
                    OnInitializationError($"More than one module can't manage {modifier.Description}");
                }
                else
                {
                    partModifiers.Add(modifier);
                    aspectLocks.Add(modifier.PartAspectLock);
                }
            }

            if (maxTemp > 0)
                MaybeAddModifier(new PartMaxTempModifier(part, partPrefab.maxTemp, maxTemp));

            if (skinMaxTemp > 0)
                MaybeAddModifier(new PartSkinMaxTempModifier(part, partPrefab.skinMaxTemp, skinMaxTemp));

            if (crashTolerance > 0)
                MaybeAddModifier(new PartCrashToleranceModifier(part, partPrefab.crashTolerance, crashTolerance));

            if (attachNode.IsNotNull())
            {
                if (part.attachRules.srfAttach)
                {
                    if (part.srfAttachNode.IsNotNull())
                        MaybeAddModifier(new PartAttachNodeModifier(part.srfAttachNode, partPrefab.srfAttachNode, attachNode, parent));
                    else
                        OnInitializationError("attachNode specified but part does not have a surface attach node");
                }
                else
                {
                    OnInitializationError("attachNode specified but part does not allow surface attach");
                }
            }

            if (CoMOffset.IsFinite())
                MaybeAddModifier(new PartCoMOffsetModifier(part, partPrefab.CoMOffset, CoMOffset));

            if (CoPOffset.IsFinite())
                MaybeAddModifier(new PartCoPOffsetModifier(part, partPrefab.CoPOffset, CoPOffset));

            if (CoLOffset.IsFinite())
                MaybeAddModifier(new PartCoLOffsetModifier(part, partPrefab.CoLOffset, CoLOffset));

            if (CenterOfBuoyancy.IsFinite())
                MaybeAddModifier(new PartCenterOfBuoyancyModifier(part, partPrefab.CenterOfBuoyancy, CenterOfBuoyancy));

            if (CenterOfDisplacement.IsFinite())
                MaybeAddModifier(new PartCenterOfDisplacementModifier(part, partPrefab.CenterOfDisplacement, CenterOfDisplacement));

            if (stackSymmetry >= 0)
                MaybeAddModifier(new PartStackSymmetryModifier(part, partPrefab.stackSymmetry, stackSymmetry));

            foreach (AttachNodeModifierInfo info in attachNodeModifierInfos)
            {
                MaybeAddModifier(info.CreateAttachNodeModifier(part, parent, OnInitializationError));
            }

            foreach (TextureSwitchInfo info in textureSwitches)
            {
                foreach(TextureReplacement replacement in info.CreateTextureReplacements(part, OnInitializationError))
                {
                    MaybeAddModifier(replacement);
                }
            }

            nodes.Clear();
            foreach (string nodeName in nodeNames)
            {
                string pattern = '^' + Regex.Escape(nodeName).Replace(@"\*", ".*").Replace(@"\?", ".") + '$';
                Regex nodeIdRegex = new Regex(pattern);

                bool foundNode = false;

                foreach (AttachNode node in part.attachNodes)
                {
                    if (!nodeIdRegex.IsMatch(node.id)) continue;

                    foundNode = true;

                    if (node.nodeType != AttachNode.NodeType.Stack)
                    {
                        OnInitializationError($"Node {node.id} is not a stack node, and thus cannot be managed by ModuleB9PartSwitch");
                        continue;
                    }

                    nodes.Add(node);
                    partModifiers.Add(new AttachNodeToggler(node));
                }

                if (!foundNode) OnInitializationError($"No attach nodes matching '{nodeName}' found");
            }

            if (HasTank)
            {
                foreach (TankResource resource in tankType)
                {
                    float filledProportion = (resource.percentFilled ?? percentFilled ?? tankType.percentFilled ?? 100f) * 0.01f;
                    bool? tweakable = resourcesTweakable ?? tankType.resourcesTweakable;
                    ResourceModifier resourceModifier = new ResourceModifier(resource, () => parent.GetTotalVolume(this), part, filledProportion, tweakable);
                    MaybeAddModifier(resourceModifier);
                }
            }

            transforms.Clear();
            foreach (var transformName in transformNames)
            {
                bool foundTransform = false;

                foreach (Transform transform in part.GetModelTransforms(transformName))
                {
                    foundTransform = true;
                    partModifiers.Add(new TransformToggler(transform, part));
                    transforms.Add(transform);
                }

                if (!foundTransform)
                    OnInitializationError($"No transforms named '{transformName}' found");
            }

            foreach (TransformModifierInfo transformModifierInfo in transformModifierInfos)
            {
                foreach (IPartModifier partModifier in transformModifierInfo.CreatePartModifiers(part, OnInitializationError))
                {
                    MaybeAddModifier(partModifier);
                }
            }

            // Icon setup doesn't set partInfo correctly, so it exists but as a copy without partConfig
            if ((part.partInfo?.partConfig).IsNotNull())
            {
                foreach (ModuleModifierInfo moduleModifierInfo in moduleModifierInfos)
                {
                    try
                    {
                        foreach (IPartModifier partModifier in moduleModifierInfo.CreatePartModifiers(part, parent))
                        {
                            MaybeAddModifier(partModifier);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnInitializationError(ex.Message);
                        Debug.LogException(ex);
                    }
                }
            }

            if (!parent.subtypes.Any(subtype => subtype.Name == mirrorSymmetrySubtype))
            {
                OnInitializationError($"Cannot find subtype '{mirrorSymmetrySubtype}' for mirror symmetry subtype");
                mirrorSymmetrySubtype = Name;
            }

            if (errorString.IsNotNull())
                SeriousWarningHandler.DisplaySeriousWarning(errorString);
        }

        #endregion

        #region Public Methods

        public void DeactivateOnStart()
        {
            if (HighLogic.LoadedSceneIsEditor)
                partModifiers.ForEach(modifier => modifier.DeactivateOnStartEditor());
            else
                partModifiers.ForEach(modifier => modifier.DeactivateOnStartFlight());
        }

        public void ActivateOnStart()
        {
            if (HighLogic.LoadedSceneIsEditor)
                partModifiers.ForEach(modifier => modifier.ActivateOnStartEditor());
            else
                partModifiers.ForEach(modifier => modifier.ActivateOnStartFlight());
        }

        public void ActivateOnStartFinished()
        {
            if (HighLogic.LoadedSceneIsEditor)
                partModifiers.ForEach(modifier => modifier.ActivateOnStartFinishedEditor());
            else
                partModifiers.ForEach(modifier => modifier.ActivateOnStartFinishedFlight());
        }

        public void DeactivateOnStartFinished()
        {
            if (HighLogic.LoadedSceneIsEditor)
                partModifiers.ForEach(modifier => modifier.DeactivateOnStartFinishedEditor());
            else
                partModifiers.ForEach(modifier => modifier.DeactivateOnStartFinishedFlight());
        }

        public void DeactivateOnSwitch()
        {
            if (HighLogic.LoadedSceneIsEditor)
                partModifiers.ForEach(modifier => modifier.DeactivateOnSwitchEditor());
            else
                partModifiers.ForEach(modifier => modifier.DeactivateOnSwitchFlight());
        }

        public void ActivateOnSwitch()
        {
            if (HighLogic.LoadedSceneIsEditor)
                partModifiers.ForEach(modifier => modifier.ActivateOnSwitchEditor());
            else
                partModifiers.ForEach(modifier => modifier.ActivateOnSwitchFlight());
        }

        public void DeactivateForIcon()
        {
            partModifiers.ForEach(modifier => modifier.OnIconCreateInactiveSubtype());
        }

        public void ActivateForIcon()
        {
            partModifiers.ForEach(modifier => modifier.OnIconCreateActiveSubtype());
        }

        public void UpdateVolume()
        {
            if (HighLogic.LoadedSceneIsEditor)
                partModifiers.ForEach(modifier => modifier.UpdateVolumeEditor());
            else
                partModifiers.ForEach(modifier => modifier.UpdateVolumeFlight());
        }

        public void OnWillBeCopiedActiveSubtype()
        {
            partModifiers.ForEach(modifier => modifier.OnWillBeCopiedActiveSubtype());
        }

        public void OnWillBeCopiedInactiveSubtype()
        {
            partModifiers.ForEach(modifier => modifier.OnWillBeCopiedInactiveSubtype());
        }

        public void OnWasCopiedActiveSubtype()
        {
            partModifiers.ForEach(modifier => modifier.OnWasCopiedActiveSubtype());
        }

        public void OnWasCopiedInactiveSubtype()
        {
            partModifiers.ForEach(modifier => modifier.OnWasCopiedInactiveSubtype());
        }

        public bool TransformIsManaged(Transform transform) => transforms.Contains(transform);
        public bool NodeManaged(AttachNode node) => nodes.Contains(node);

        public bool ModuleShouldBeEnabled(PartModule module)
        {
            foreach (IPartModifier partModifier in partModifiers)
            {
                if (!(partModifier is ModuleDeactivator moduleDeactivator)) continue;
                if (moduleDeactivator.module == module) return false;
            }

            return true;
        }

        public void AssignStructuralTankType()
        {
            if (!tankType.IsStructuralTankType)
                tankType = B9TankSettings.StructuralTankType;
        }

        public bool IsUnlocked()
        {
            if (!HasUpgradeRequired) return true;
            if (HighLogic.CurrentGame.IsNull()) return true;
            if (HighLogic.CurrentGame.Mode == Game.Modes.SANDBOX) return true;
            return PartUpgradeManager.Handler.IsUnlocked(upgradeRequired);
        }

        public override string ToString()
        {
            string log = "PartSubtype";
            if (!Name.IsNullOrEmpty())
                log += $" {Name}";
            if (parent != null)
                log += $" on module {parent}";
            return log;
        }

        #endregion

        #region Private Methods

        private void LoadAdditionalResources(ConfigNode[] resourceNodes, OperationContext context)
        {
            OperationContext newContext = new OperationContext(context, this);
            foreach (ConfigNode resourceNode in resourceNodes)
            {
                string name = resourceNode.GetValue("name");

                if (name.IsNullOrEmpty())
                {
                    LogError("Cannot load a RESOURCE node without a name");
                    continue;
                }

                TankResource resource = tankType[name];

                if (resource.IsNull())
                {
                    resource = new TankResource();
                    tankType.resources.Add(resource);
                }

                resource.Load(resourceNode, newContext);
            }
        }

        #region Logging

        private void LogWarning(string message) => Debug.LogWarning($"Warning on {this}: {message}");

        private void LogError(string message) => Debug.LogWarning($"Warning on {this}: {message}");

        #endregion

        #endregion
    }
}
