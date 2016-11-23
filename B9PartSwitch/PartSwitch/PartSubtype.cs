using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

namespace B9PartSwitch
{
    public struct TransformInfo
    {
        public readonly Transform transform;
        public readonly GameObject gameObject;
        public readonly Collider collider;

        public TransformInfo(Transform t)
        {
            transform = t;
            gameObject = transform?.gameObject;
            collider = t?.GetComponent<Collider>();
        }

        public void Enable()
        {
            gameObject?.SetActive(true);

            if (collider != null)
                collider.enabled = true;
        }

        public void Disable()
        {
            gameObject?.SetActive(false);

            if (collider != null)
                collider.enabled = false;
        }
    }
    
    public class PartSubtype : CFGUtilObject
    {
        #region Config Fields

        [ConfigField(configName = "name")]
        public string subtypeName;

        [ConfigField]
        public string title;

        [ConfigField(configName = "transform")]
        public List<string> transformNames = new List<string>();

        [ConfigField(configName = "node")]
        public List<string> nodeNames = new List<string>();

        [ConfigField]
        public float addedMass = 0f;

        [ConfigField]
        public float addedCost = 0f;

        [ConfigField]
        public TankType tankType;

        [ConfigField]
        public float volumeMultiplier = 1f;

        [ConfigField]
        public float volumeAdded = 0f;

        [ConfigField]
        public float maxTemp;

        [ConfigField]
        public float skinMaxTemp;

        [ConfigField]
        public AttachNode attachNode = null;

        [ConfigField]
        public float crashTolerance = 0f;

        [ConfigField]
        public Vector3 CoMOffset = Vector3Extensions.NaN();

        [ConfigField]
        public Vector3 CoPOffset = Vector3Extensions.NaN();

        [ConfigField]
        public Vector3 CoLOffset = Vector3Extensions.NaN();

        [ConfigField]
        public Vector3 CenterOfBuoyancy = Vector3Extensions.NaN();

        [ConfigField]
        public Vector3 CenterOfDisplacement = Vector3Extensions.NaN();

        #endregion

        #region Private Fields

        private ModuleB9PartSwitch parent;
        private List<TransformInfo> transforms = new List<TransformInfo>();
        private List<AttachNode> nodes = new List<AttachNode>();

        #endregion

        #region Properties

        public string Name => subtypeName;

        public Part Part => parent.part;

        public PartSubtypeContext Context => new PartSubtypeContext(Part, parent, this);

        public bool HasTank => tankType != null && tankType.ResourcesCount > 0;

        public IEnumerable<Transform> Transforms => transforms.Select(transform => transform.transform);
        public IEnumerable<AttachNode> Nodes => nodes.All();
        public IEnumerable<string> ResourceNames => tankType.ResourceNames;
        public IEnumerable<string> NodeIDs => nodes.Select(n => n.id);

        public float TotalVolume => HasTank ? ((parent?.baseVolume ?? 0f) * volumeMultiplier + volumeAdded) : 0f;

        public float TotalMass => TotalVolume * tankType.tankMass + addedMass;
        public float TotalCost => TotalVolume * tankType.TotalUnitCost + addedCost;

        public bool ChangesMass => (addedMass != 0f) || tankType.ChangesMass;
        public bool ChangesCost => (addedCost != 0f) || tankType.ChangesCost;

        #endregion

        #region Setup

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (tankType == null)
                tankType = B9TankSettings.StructuralTankType;

            if (string.IsNullOrEmpty(title))
                title = subtypeName;
        }

        public void Setup(ModuleB9PartSwitch parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent cannot be null");
            if (parent.part == null)
                throw new ArgumentNullException("parent.part cannot be null");

            this.parent = parent;

            FindObjects();
            FindNodes();
        }

        public void FindObjects()
        {
            if (parent == null)
                throw new InvalidOperationException("Parent has not been set");

            transforms = new List<TransformInfo>();
            foreach (var transformName in transformNames)
            {
                Transform[] tempTransforms = Part.FindModelTransforms(transformName);
                if (tempTransforms == null || tempTransforms.Length == 0)
                    LogError($"No transforms named {transformName} found");
                else
                    transforms.AddRange(tempTransforms.Select(t => new TransformInfo(t)));
            }
        }

        public void FindNodes()
        {
            if (parent == null)
                throw new InvalidOperationException("Parent has not been set");

            nodes = new List<AttachNode>();
            foreach (var nodeName in nodeNames)
            {
                AttachNode[] tempNodes = Part.FindAttachNodes(nodeName);
                if (tempNodes == null || tempNodes.Length == 0)
                {
                    LogError($"No attach nodes matching {nodeName} found");
                }
                else
                {
                    foreach (var node in tempNodes)
                    {
                        // If a node has been deactivated then it will be a docking node
                        // Alternative: activate all nodes on serialization
                        if (node.nodeType == AttachNode.NodeType.Stack || node.nodeType == AttachNode.NodeType.Dock)
                            nodes.Add(node);
                        else
                            LogError($"Node {node.id} is not a stack node, and thus cannot be managed by ModuleB9PartSwitch (found by node identifier {nodeName})");
                    }
                }
            }
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
            AddResources(false);
            UpdatePartParams();
        }

        public void DeactivateOnSwitch()
        {
            DeactivateObjects();

            if (HighLogic.LoadedSceneIsEditor)
                DeactivateNodes();
            else
                ActivateNodes();

            RemoveResources();
        }

        public void ActivateOnSwitch()
        {
            ActivateObjects();
            ActivateNodes();
            AddResources(true);
            UpdatePartParams();
        }

        public void ActivateObjects() => transforms.ForEach(t => t.Enable());

        public void ActivateNodes() => nodes.ForEach(n => n.Unhide());

        public void DeactivateObjects() => transforms.ForEach(t => t.Disable());

        public void DeactivateNodes() => nodes.ForEach(n => n.Hide());

        public void AddResources(bool fillTanks)
        {
            foreach (TankResource resource in tankType.resources)
            {
                float amount = TotalVolume * resource.unitsPerVolume * parent.VolumeScale;
                Part.AddOrCreateResource(resource.resourceDefinition, amount, fillTanks ? amount : -1f);
            }
        }

        public void RemoveResources()
        {
            foreach (TankResource resource in tankType.resources)
            {
                Part.RemoveResource(resource.ResourceName);
            }
        }

        public bool TransformIsManaged(Transform transform) => transforms.Any(info => object.ReferenceEquals(info.transform, transform));
        public bool NodeManaged(AttachNode node) => nodes.Contains(node);
        public bool ResourceManaged(String resourceName) => ResourceNames.Contains(resourceName);

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
            if (!string.IsNullOrEmpty(Name))
                log += $" {Name}";
            if (parent != null)
                log += $" on module {parent}";
            return log;
        }

        #endregion

        #region Private Methods

        private void UpdatePartParams()
        {
            foreach (ISubtypePartField field in SubtypePartFields.All.Where(field => parent.PartFieldManaged(field)))
            {
                field.AssignValueOnSubtype(Context);
            }
        }

        #region Logging

        private void LogWarning(string message) => Debug.LogWarning($"Warning on {this}: {message}");

        private void LogError(string message) => Debug.LogWarning($"Warning on {this}: {message}");

        #endregion

        #endregion
    }
}
