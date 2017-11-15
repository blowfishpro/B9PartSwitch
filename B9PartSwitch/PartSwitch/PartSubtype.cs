﻿using System;
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

        #endregion

        #region Interface Methods

        public void Load(ConfigNode node, OperationContext context)
        {
            OperationContext newContext = this.LoadFields(node, context);

            OnLoad(node, newContext);
        }

        public void Save(ConfigNode node, OperationContext context) => this.SaveFields(node, context);

        #endregion

        #region Setup

        private void OnLoad(ConfigNode node, OperationContext context)
        {
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

            FindObjects();
            FindNodes();
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

        public void ActivateObjects() => transforms.ForEach(t => Part.UpdateTransformEnabled(t));

        public void ActivateNodes() => nodes.ForEach(n => Part.UpdateNodeEnabled(n));

        public void DeactivateObjects() => transforms.ForEach(t => t.Disable());

        public void DeactivateNodes() => nodes.ForEach(n => n.Hide());

        public void AddResources(bool fillTanks)
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

        public void RemoveResources()
        {
            foreach (TankResource resource in tankType.resources)
            {
                Part.RemoveResource(resource.ResourceName);
            }
        }

        public bool TransformIsManaged(Transform transform) => transforms.Contains(transform);
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
