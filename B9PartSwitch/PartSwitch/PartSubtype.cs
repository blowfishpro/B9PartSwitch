using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch
{
    public class PartSubtype : IContextualNode
    {
        #region Config Fields

        [NodeData(name = "name")]
        public string subtypeName;

        [NodeData]
        public string title;

        [NodeData(name = "transform")]
        public List<string> transformNames = new List<string>();

        [NodeData(name = "node")]
        public List<string> nodeNames = new List<string>();

        [NodeData(name = "TEXTURE")]
        public List<TextureSwitchInfo> textureSwitches = new List<TextureSwitchInfo>();

        [NodeData(name = "NODE")]
        public List<AttachNodeModifierInfo> attachNodeModifierInfos = new List<AttachNodeModifierInfo>();

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

        #endregion

        #region Private Fields

        private ModuleB9PartSwitch parent;
        private List<Transform> transforms = new List<Transform>();
        private List<AttachNode> nodes = new List<AttachNode>();
        private List<TextureReplacement> textureReplacements = new List<TextureReplacement>();
        private List<AttachNodeMover> attachNodeMovers = new List<AttachNodeMover>();
        private List<IPartModifier> partModifiers = new List<IPartModifier>();
        private List<object> aspectLocks = new List<object>();

        #endregion

        #region Properties

        public string Name => subtypeName;

        public Part Part => parent.part;

        public bool HasTank => tankType != null && tankType.ResourcesCount > 0;

        public IEnumerable<Transform> Transforms => transforms.Select(transform => transform.transform);
        public IEnumerable<AttachNode> Nodes => nodes.All();
        public IEnumerable<string> ResourceNames => tankType.ResourceNames;
        public IEnumerable<string> NodeIDs => nodes.Select(n => n.id);
        public IEnumerable<Material> Materials => textureReplacements.Select(repl => repl.material);
        public IEnumerable<AttachNode> AttachNodesWithManagedPosition => attachNodeMovers.Select(mover => mover.attachNode);

        public float TotalVolume
        {
            get
            {
                if (parent.IsNull()) throw new InvalidOperationException("Cannot get volume before parent has been linked!");

                if (!HasTank) return 0f;
                return parent.baseVolume * volumeMultiplier + volumeAdded + parent.VolumeFromChildren;
            }
        }

        public float TotalMass => TotalVolume * tankType.tankMass + addedMass;
        public float TotalCost => TotalVolume * tankType.TotalUnitCost + addedCost;

        public bool ChangesMass => (addedMass != 0f) || tankType.ChangesMass;
        public bool ChangesCost => (addedCost != 0f) || tankType.ChangesCost;

        public IEnumerable<object> PartAspectLocks => aspectLocks.All();

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

            if (tankType == null)
                tankType = B9TankSettings.StructuralTankType;

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

        public void Setup(ModuleB9PartSwitch parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent cannot be null");
            if (parent.part == null)
                throw new ArgumentNullException("parent.part cannot be null");

            this.parent = parent;

            aspectLocks.Clear();

            FindObjects();
            FindNodes();
            FindTextureReplacements();
            FindAttachNodeMovers();

            Part partPrefab = Part.GetPrefab() ?? Part;

            IEnumerable<object> aspectLocksOnOtherModules = parent.PartAspectLocksOnOtherModules;

            void MaybeAddModifier(IPartModifier modifier)
            {
                if (aspectLocksOnOtherModules.Contains(modifier.PartAspectLock))
                {
                    LogError($"More than one module can't manage {modifier.Description}");
                }
                else
                {
                    partModifiers.Add(modifier);
                    aspectLocks.Add(modifier.PartAspectLock);
                }
            }

            if (maxTemp > 0)
                MaybeAddModifier(new PartMaxTempModifier(Part, partPrefab.maxTemp, maxTemp));

            if (skinMaxTemp > 0)
                MaybeAddModifier(new PartSkinMaxTempModifier(Part, partPrefab.skinMaxTemp, skinMaxTemp));

            if (crashTolerance > 0)
                MaybeAddModifier(new PartCrashToleranceModifier(Part, partPrefab.crashTolerance, crashTolerance));

            if (Part.attachRules.allowSrfAttach && Part.srfAttachNode.IsNull() && attachNode != null)
                MaybeAddModifier(new PartAttachNodeModifier(Part.srfAttachNode, partPrefab.srfAttachNode, attachNode));

            if (CoMOffset.IsFinite())
                MaybeAddModifier(new PartCoMOffsetModifier(Part, partPrefab.CoMOffset, CoMOffset));

            if (CoPOffset.IsFinite())
                MaybeAddModifier(new PartCoPOffsetModifier(Part, partPrefab.CoPOffset, CoPOffset));

            if (CoLOffset.IsFinite())
                MaybeAddModifier(new PartCoLOffsetModifier(Part, partPrefab.CoLOffset, CoLOffset));

            if (CenterOfBuoyancy.IsFinite())
                MaybeAddModifier(new PartCenterOfBuoyancyModifier(Part, partPrefab.CenterOfBuoyancy, CenterOfBuoyancy));

            if (CenterOfDisplacement.IsFinite())
                MaybeAddModifier(new PartCenterOfDisplacementModifier(Part, partPrefab.CenterOfDisplacement, CenterOfDisplacement));

            if (stackSymmetry >= 0)
                MaybeAddModifier(new PartStackSymmetryModifier(Part, partPrefab.stackSymmetry, stackSymmetry));
        }

        #endregion

        #region Public Methods

        public void DeactivateOnStart()
        {
            DeactivateObjects();

            if (HighLogic.LoadedSceneIsEditor)
                DeactivateNodes();
            else
                ActivateNodes();
        }

        public void ActivateOnStart()
        {
            ActivateObjects();
            ActivateNodes();
            ActivateTextures();
            AddResources(false);
            SetPartParams();
            attachNodeMovers.ForEach(nm => nm.ActivateOnStart());
        }

        public void ActivateAfterStart()
        {
            attachNodeMovers.ForEach(nm => nm.ActivateAfterStart());
        }

        public void DeactivateOnSwitch()
        {
            DeactivateObjects();

            if (HighLogic.LoadedSceneIsEditor)
                DeactivateNodes();
            else
                ActivateNodes();

            DeactivateTextures();
            RemoveResources();
            UnsetPartParams();
            attachNodeMovers.ForEach(nm => nm.DeactivateOnSwitch());
        }

        public void ActivateOnSwitch()
        {
            ActivateObjects();
            ActivateNodes();
            ActivateTextures();
            AddResources(true);
            SetPartParams();
            attachNodeMovers.ForEach(nm => nm.ActivateOnSwitch());
        }

        public void DeactivateForIcon()
        {
            DeactivateObjects();
        }

        public void ActivateForIcon()
        {
            ActivateObjects();
            ActivateTextures();
        }

        public void UpdateVolume()
        {
            AddResources(true);
        }

        public void OnWillBeCopiedActiveSubtype()
        {
            DeactivateTextures();
        }

        public void OnWillBeCopiedInactiveSubtype()
        {
            ActivateNodes();
        }

        public void OnWasCopiedActiveSubtype()
        {
            ActivateTextures();
            ActivateNodes();
        }

        public void OnWasCopiedInactiveSubtype()
        {
            DeactivateNodes();
        }

        public bool TransformIsManaged(Transform transform) => transforms.Contains(transform);
        public bool NodeManaged(AttachNode node) => nodes.Contains(node);
        public bool ResourceManaged(String resourceName) => ResourceNames.Contains(resourceName);
        public bool MaterialIsManaged(Material material) => textureReplacements.Any(repl => repl.material == material);

        public void AssignStructuralTankType()
        {
            if (!tankType.IsStructuralTankType)
                tankType = B9TankSettings.StructuralTankType;
        }

        public void ClearAttachNode()
        {
            attachNode = null;
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

        private void FindObjects()
        {
            if (parent == null)
                throw new InvalidOperationException("Parent has not been set");

            transforms.Clear();
            foreach (var transformName in transformNames)
            {
                Transform[] tempTransforms = Part.FindModelTransforms(transformName);
                if (tempTransforms == null || tempTransforms.Length == 0)
                    LogError($"No transforms named {transformName} found");
                else
                    transforms.AddRange(tempTransforms);
            }
        }

        private void FindNodes()
        {
            if (parent == null)
                throw new InvalidOperationException("Parent has not been set");

            nodes = new List<AttachNode>();
            foreach (var nodeName in nodeNames)
            {
                bool foundNode = false;

                foreach (AttachNode node in Part.attachNodes.Where(node => node.id == nodeName))
                {
                    foundNode = true;

                    // If a node has been deactivated then it will be a docking node
                    // Alternative: activate all nodes on serialization
                    if (node.nodeType == AttachNode.NodeType.Stack || node.nodeType == AttachNode.NodeType.Dock)
                        nodes.Add(node);
                    else
                        LogError($"Node {node.id} is not a stack node, and thus cannot be managed by ModuleB9PartSwitch");
                }

                if (!foundNode) LogError($"No attach nodes matching {nodeName} found");
            }
        }

        private void FindTextureReplacements()
        {
            if (parent == null)
                throw new InvalidOperationException("Parent has not been set");

            // Ensure that textures are reset before doing this
            foreach(TextureReplacement tr in textureReplacements)
            {
                tr.Deactivate();
            }

            textureReplacements.Clear();

            foreach (TextureSwitchInfo info in textureSwitches)
            {
                try
                {
                    textureReplacements.AddRange(info.CreateTextureReplacements(Part));
                }
                catch(Exception e)
                {
                    LogError("Exception while initializing a texture replacment:");
                    Debug.LogException(e);
                }
            }
        }

        private void FindAttachNodeMovers()
        {
            foreach (AttachNodeModifierInfo info in attachNodeModifierInfos)
            {
                try
                {
                    AttachNodeMover mover = info.CreateAttachNodeModifier(Part, parent);
                    if (mover != null) attachNodeMovers.Add(mover);
                }
                catch (Exception e)
                {
                    LogError("Exception while initializing a node mover:");
                    Debug.LogException(e);
                }
            }
        }

        private void SetPartParams()
        {
            partModifiers.ForEach(modifier => modifier.Activate());
        }

        private void UnsetPartParams()
        {
            partModifiers.ForEach(modifier => modifier.Deactivate());
        }

        private void ActivateObjects() => transforms.ForEach(t => Part.UpdateTransformEnabled(t));
        private void ActivateNodes() => nodes.ForEach(n => Part.UpdateNodeEnabled(n));
        private void ActivateTextures() => textureReplacements.ForEach(t => t.Activate());
        private void DeactivateObjects() => transforms.ForEach(t => t.Disable());
        private void DeactivateNodes() => nodes.ForEach(n => n.Hide());
        private void DeactivateTextures() => textureReplacements.ForEach(t => t.Deactivate());

        private void AddResources(bool fillTanks)
        {
            foreach (TankResource resource in tankType.resources)
            {
                float amount = TotalVolume * resource.unitsPerVolume * parent.VolumeScale;
                float filledProportion;
                if (HighLogic.LoadedSceneIsFlight && fillTanks)
                    filledProportion = 0;
                else
                    filledProportion = (resource.percentFilled ?? percentFilled ?? tankType.percentFilled ?? 100f) * 0.01f;
                PartResource partResource = Part.AddOrCreateResource(resource.resourceDefinition, amount, amount * filledProportion, fillTanks);

                bool? tweakable = resourcesTweakable ?? tankType.resourcesTweakable;

                if (tweakable.HasValue)
                    partResource.isTweakable = tweakable.Value;
            }
        }

        private void RemoveResources()
        {
            foreach (TankResource resource in tankType.resources)
            {
                Part.RemoveResource(resource.ResourceName);
            }
        }

        #region Logging

        private void LogWarning(string message) => Debug.LogWarning($"Warning on {this}: {message}");

        private void LogError(string message) => Debug.LogWarning($"Warning on {this}: {message}");

        #endregion

        #endregion
    }

    public interface IPartModifier
    {
        object PartAspectLock { get; }
        string Description { get; }
        void Activate();
        void Deactivate();
    }

    public class PartMaxTempModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "maxTemp";

        private readonly Part part;
        private readonly double origMaxTemp;
        private readonly double newMaxTemp;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's maxTemp";

        public PartMaxTempModifier(Part part, double origMaxTemp, double newMaxTemp)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origMaxTemp = origMaxTemp;
            this.newMaxTemp = newMaxTemp;
        }

        public void Activate()
        {
            part.maxTemp = newMaxTemp;
        }

        public void Deactivate()
        {
            part.maxTemp = origMaxTemp;
        }
    }

    public class PartSkinMaxTempModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "skinMaxTemp";

        private readonly Part part;
        private readonly double origSkinMaxTemp;
        private readonly double newSkinMaxTemp;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's skinMaxTemp";

        public PartSkinMaxTempModifier(Part part, double origSkinMaxTemp, double newSkinMaxTemp)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origSkinMaxTemp = origSkinMaxTemp;
            this.newSkinMaxTemp = newSkinMaxTemp;
        }

        public void Activate()
        {
            part.skinMaxTemp = newSkinMaxTemp;
        }

        public void Deactivate()
        {
            part.skinMaxTemp = origSkinMaxTemp;
        }
    }

    public class PartCrashToleranceModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "crashTolerance";

        private readonly Part part;
        private readonly float origCrashTolerance;
        private readonly float newCrashTolerance;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's crashTolerance";

        public PartCrashToleranceModifier(Part part, float origCrashTolerance, float newCrashTolerance)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCrashTolerance = origCrashTolerance;
            this.newCrashTolerance = newCrashTolerance;
        }

        public void Activate()
        {
            part.crashTolerance = newCrashTolerance;
        }

        public void Deactivate()
        {
            part.crashTolerance = origCrashTolerance;
        }
    }

    public class PartAttachNodeModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "attachNode";

        private readonly AttachNode partAttachNode;
        private readonly AttachNode referenceAttachNode;
        private readonly AttachNode newAttachNode;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's surface attach node";

        public PartAttachNodeModifier(AttachNode partAttachNode, AttachNode referenceAttachNode, AttachNode newAttachNode)
        {
            partAttachNode.ThrowIfNullArgument(nameof(partAttachNode));

            this.partAttachNode = partAttachNode;
            this.referenceAttachNode = referenceAttachNode;
            this.newAttachNode = newAttachNode;
        }

        public void Activate()
        {
            partAttachNode.position = referenceAttachNode.position;
            partAttachNode.orientation = referenceAttachNode.orientation;
        }

        public void Deactivate()
        {
            partAttachNode.position = referenceAttachNode.position;
            partAttachNode.orientation = referenceAttachNode.orientation;
        }
    }

    public class PartCoMOffsetModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "CoMOffset";

        private readonly Part part;
        private readonly Vector3 origCoMOffset;
        private readonly Vector3 newCoMOffset;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's CoMOffset";

        public PartCoMOffsetModifier(Part part, Vector3 origCoMOffset, Vector3 newCoMOffset)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCoMOffset = origCoMOffset;
            this.newCoMOffset = newCoMOffset;
        }

        public void Activate()
        {
            part.CoMOffset = newCoMOffset;
        }

        public void Deactivate()
        {
            part.CoMOffset = origCoMOffset;
        }
    }

    public class PartCoPOffsetModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "CoPOffset";

        private readonly Part part;
        private readonly Vector3 origCoPOffset;
        private readonly Vector3 newCoPOffset;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's CoPOffset";

        public PartCoPOffsetModifier(Part part, Vector3 origCoPOffset, Vector3 newCoPOffset)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCoPOffset = origCoPOffset;
            this.newCoPOffset = newCoPOffset;
        }

        public void Activate()
        {
            part.CoPOffset = newCoPOffset;
        }

        public void Deactivate()
        {
            part.CoPOffset = origCoPOffset;
        }
    }

    public class PartCoLOffsetModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "CoLOffset";

        private readonly Part part;
        private readonly Vector3 origCoLOffset;
        private readonly Vector3 newCoLOffset;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's CoLOffset";

        public PartCoLOffsetModifier(Part part, Vector3 origCoLOffset, Vector3 newCoLOffset)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCoLOffset = origCoLOffset;
            this.newCoLOffset = newCoLOffset;
        }

        public void Activate()
        {
            part.CoLOffset = newCoLOffset;
        }

        public void Deactivate()
        {
            part.CoLOffset = origCoLOffset;
        }
    }

    public class PartCenterOfDisplacementModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "CenterOfDisplacement";

        private readonly Part part;
        private readonly Vector3 origCenterOfDisplacement;
        private readonly Vector3 newCenterOfDisplacement;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's CenterOfDisplacement";

        public PartCenterOfDisplacementModifier(Part part, Vector3 origCenterOfDisplacement, Vector3 newCenterOfDisplacement)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCenterOfDisplacement = origCenterOfDisplacement;
            this.newCenterOfDisplacement = newCenterOfDisplacement;
        }

        public void Activate()
        {
            part.CenterOfDisplacement = newCenterOfDisplacement;
        }

        public void Deactivate()
        {
            part.CenterOfDisplacement = origCenterOfDisplacement;
        }
    }

    public class PartCenterOfBuoyancyModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "CenterOfBuoyancy";

        private readonly Part part;
        private readonly Vector3 origCenterOfBuoyancy;
        private readonly Vector3 newCenterOfBuoyancy;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's CenterOfBuoyancy";

        public PartCenterOfBuoyancyModifier(Part part, Vector3 origCenterOfBuoyancy, Vector3 newCenterOfBuoyancy)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origCenterOfBuoyancy = origCenterOfBuoyancy;
            this.newCenterOfBuoyancy = newCenterOfBuoyancy;
        }

        public void Activate()
        {
            part.CenterOfBuoyancy = newCenterOfBuoyancy;
        }

        public void Deactivate()
        {
            part.CenterOfBuoyancy = origCenterOfBuoyancy;
        }
    }

    public class PartStackSymmetryModifier : IPartModifier
    {
        public const string PART_ASPECT_LOCK = "stackSymmetry";

        private readonly Part part;
        private readonly int origStackSymmetry;
        private readonly int newStackSymmetry;

        public object PartAspectLock => PART_ASPECT_LOCK;
        public string Description => "a part's stackSymmetry";

        public PartStackSymmetryModifier(Part part, int origStackSymmetry, int newStackSymmetry)
        {
            part.ThrowIfNullArgument(nameof(part));

            this.part = part;
            this.origStackSymmetry = origStackSymmetry;
            this.newStackSymmetry = newStackSymmetry;
        }

        public void Activate()
        {
            part.stackSymmetry = newStackSymmetry;
        }

        public void Deactivate()
        {
            part.stackSymmetry = origStackSymmetry;
        }
    }
}
