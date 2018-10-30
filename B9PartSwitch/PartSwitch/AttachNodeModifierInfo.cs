using System;
using System.Linq;
using UnityEngine;
using B9PartSwitch.Fishbones;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch
{
    public class AttachNodeModifierInfo : IContextualNode
    {
        [NodeData(name = "name")]
        public string nodeID;

        [NodeData]
        public Vector3? position;

        public void Load(ConfigNode node, OperationContext context)
        {
            this.LoadFields(node, context);
        }

        public void Save(ConfigNode node, OperationContext context)
        {
            this.SaveFields(node, context);
        }

        public AttachNodeMover CreateAttachNodeModifier(Part part)
        {
            if (position == null) return null;
            AttachNode node = part.attachNodes.FirstOrDefault(n => (n.nodeType == AttachNode.NodeType.Stack || n.nodeType == AttachNode.NodeType.Dock) && n.id == nodeID);

            if (node == null)
            {
                part.LogError($"Attach node with id '{nodeID}' not found for attach node modifier");
                return null;
            }

            return new AttachNodeMover(node, position.Value);
        }
    }
}
