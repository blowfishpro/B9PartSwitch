using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace B9PartSwitch
{
    [Serializable]
    public class PartSubtype : CFGUtilObject
    {
        #region Config Fields

        [ConfigField(configName = "name")]
        public string subtypeName;

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

        #endregion

        #region Private Fields

        private ModuleB9PartSwitch parent;
        private Part part;
        private List<Transform> transforms = new List<Transform>();
        private List<AttachNode> nodes = new List<AttachNode>();

        #endregion

        #region Properties

        public string Name { get { return subtypeName; } }

        #endregion

        #region Setup

        public override void OnLoad(ConfigNode node)
        {
            base.OnLoad(node);

            if (tankType == null)
            {
                tankType = MiniMFTSettings.CloneTankType(MiniMFTSettings.StructuralTankType, gameObject);
            }

            if (attachNode != null)
                attachNode.id = "attach-node";

        }

        public void SetParent(ModuleB9PartSwitch parent)
        {
            if (parent == null)
                throw new ArgumentNullException("parent cannot be null");
            if (parent.part == null)
                throw new ArgumentNullException("parent.part cannot be null");

            this.parent = parent;
            part = parent.part;
        }

        public void FindObjects()
        {
            if (parent == null)
                throw new InvalidOperationException("Parent has not been set");

            transforms = new List<Transform>();
            for (int i = 0; i < transformNames.Count; i++)
            {
                Transform[] tempTransforms = part.FindModelTransforms(transformNames[i]);
                if (tempTransforms == null || tempTransforms.Length == 0)
                    LogError("No transformes named " + transformNames[i] + " found");
                else
                    transforms.AddRange(tempTransforms);
            }
        }

        public void FindNodes()
        {
            if (parent == null)
                throw new InvalidOperationException("Parent has not been set");

            nodes = new List<AttachNode>();
            for (int i = 0; i < nodeNames.Count; i++)
            {
                AttachNode[] tempNodes = part.findAttachNodes(nodeNames[i]);
                if (tempNodes == null || tempNodes.Length == 0)
                {
                    LogError("No attach nodes matching " + nodeNames[i] + "found");
                }
                else
                {
                    for (int j = 0; j < tempNodes.Length; j++)
                    {
                        if (tempNodes[j].nodeType != AttachNode.NodeType.Stack)
                            LogError("Node " + tempNodes[j].id + " is not a stack node, and thus cannot be managed by ModuleB9PartSwitch (found by node identifier " + nodeNames[i] + ")");
                        else
                            nodes.Add(tempNodes[j]);
                    }
                }
            }
        }

        public void OnStart()
        {
            if (parent == null)
                throw new InvalidOperationException("Parent has not been set");

            // Check for empty attach node, since Unity will initialize to default value if prefab has null
            if (attachNode != null && string.IsNullOrEmpty(attachNode.id))
                attachNode = null;

            FindObjects();
            FindNodes();
        }

        #endregion

        #region Public Methods

        public IEnumerator<string> GetNodeIDs()
        {
            for (int i = 0; i < nodes.Count; i++)
                yield return nodes[i].id;
        }

        public void ActivateObjects()
        {
            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i] == null || transforms[i].gameObject == null)
                {
                    Debug.LogError("Transform is no longer valid");
                    transforms.RemoveAt(i);
                    i--;
                    continue;
                }
                transforms[i].gameObject.SetActive(true);
                if (transforms[i].gameObject.collider != null)
                    transforms[i].gameObject.collider.enabled = true;
            }
        }

        public void ActivateNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                {
                    LogError("Node is no longer valid");
                    nodes.RemoveAt(i);
                    i--;
                    continue;
                }
                nodes[i].nodeType = AttachNode.NodeType.Stack;
                nodes[i].radius = 0.4f;
            }
        }

        public void DeactivateObjects()
        {
            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i] == null || transforms[i].gameObject == null)
                {
                    LogError("Transform is no longer valid");
                    transforms.RemoveAt(i);
                    i--;
                    continue;
                }
                transforms[i].gameObject.SetActive(false);
                if (transforms[i].gameObject.collider != null)
                    transforms[i].gameObject.collider.enabled = false;
            }
        }

        public void DeactivateNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null)
                {
                    LogError("Node is no loger valid");
                    nodes.RemoveAt(i);
                    i--;
                    continue;
                }
                nodes[i].nodeType = AttachNode.NodeType.Dock;
                nodes[i].radius = 0.001f;
            }
        }

        public override string ToString()
        {
            string log = "PartSubtype";
            if (!string.IsNullOrEmpty(Name))
                log += " " + Name;
            if (parent != null)
                log += " on part " + parent.ToString();
            return log;
        }

        #endregion

        #region Private Methods

        private void LogWarning(string message)
        {
            string log = "Warning on ";
            log += this.ToString();
            log += ": ";
            log += message;
            Debug.LogWarning(message);
        }

        private void LogError(string message)
        {
            string log = "Error on ";
            log += this.ToString();
            log += ": ";
            log += message;
            Debug.LogWarning(message);
        }

        #endregion
    }
}
