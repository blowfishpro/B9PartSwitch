using System.Collections.Generic;
using static ConfigNode;

namespace B9PartSwitch.Extensions
{
    public static class ConfigNodeExtensions
    {
        private class DuplicatableValue
        {
            public Value value;
            public int sameNameIndex;

            public DuplicatableValue(Value value, int sameNameIndex)
            {
                this.value = value;
                this.sameNameIndex = sameNameIndex;
            }
        }

        private static List<DuplicatableValue> GetValuesFromNode(ConfigNode node)
        {
            List<DuplicatableValue> values = new List<DuplicatableValue>();
            for (int i = 0; i < node.values.Count; i++)
            {
                if (!values.Exists(p => p.value.name == node.values[i].name))
                {
                    values.Add(new DuplicatableValue(node.values[i], 0));
                }
                else
                {
                    values.Add(new DuplicatableValue(node.values[i], values.FindLast(p => p.value.name == node.values[i].name).sameNameIndex + 1));
                }
            }
            return values;
        }

        private class DuplicatableNode
        {
            public ConfigNode node;
            public int sameNameIndex;

            public DuplicatableNode(ConfigNode node, int sameNameIndex)
            {
                this.node = node;
                this.sameNameIndex = sameNameIndex;
            }
        }

        private static List<DuplicatableNode> GetNodesFromNode(ConfigNode baseNode)
        {
            List<DuplicatableNode> nodes = new List<DuplicatableNode>();
            for (int i = 0; i < baseNode.nodes.Count; i++)
            {
                if (!nodes.Exists(p => p.node.name == baseNode.nodes[i].name))
                {
                    nodes.Add(new DuplicatableNode(baseNode.nodes[i], 0));
                }
                else
                {
                    nodes.Add(new DuplicatableNode(baseNode.nodes[i], nodes.FindLast(p => p.node.name == baseNode.nodes[i].name).sameNameIndex + 1));
                }
            }
            return nodes;
        }

        // This whole thing generate a lot of garbage and can probably be done better but this works.
        public static void CopyToAsModifier(this ConfigNode modifierNode, ConfigNode baseNode)
        {
            if (baseNode.name == string.Empty)
            {
                baseNode.name = modifierNode.name;
            }
            if (baseNode.id == string.Empty)
            {
                baseNode.id = modifierNode.id;
            }
            if (!string.IsNullOrEmpty(modifierNode.comment))
            {
                baseNode.comment = modifierNode.comment;
            }

            // Get lists of values with an index for values that have the same name
            List<DuplicatableValue> baseValues = GetValuesFromNode(baseNode);
            List<DuplicatableValue> modifierValues = GetValuesFromNode(modifierNode);

            // Foreach base value, if it has a matching modifier value, either delete it or replace it
            // Then remove the modifier value from its list
            for (int i = baseValues.Count - 1; i >= 0 ; i--)
            {
                DuplicatableValue modValue = modifierValues.Find(p => p.value.name == baseValues[i].value.name && p.sameNameIndex == baseValues[i].sameNameIndex);
                if (modValue != null)
                {
                    if (modValue.value.value == "deleteValue")
                    {
                        baseValues.RemoveAt(i);
                    }
                    else
                    {
                        baseValues[i] = modValue;
                    }
                    modifierValues.Remove(modValue);
                }
            }

            // Now that the modifier values list only contains added values, insert them.
            for (int i = 0; i < modifierValues.Count; i++)
            {
                baseValues.Insert(baseValues.FindLastIndex(p => p.value.name == modifierValues[i].value.name) + 1, modifierValues[i]);
            }

            // Rebuild the ConfigNode values
            baseNode.values.Clear();
            for (int i = 0; i < baseValues.Count; i++)
            {
                baseNode.values.Add(baseValues[i].value);
            }

            // Get lists of nodes with an index for nodes that have the same name
            List<DuplicatableNode> baseNodes = GetNodesFromNode(baseNode);
            List<DuplicatableNode> modifierNodes = GetNodesFromNode(modifierNode);

            // Foreach base node, if it has a matching modifier node, either delete it or recursivly call this method on it
            // Then remove the modifier node from its list
            for (int i = baseNodes.Count - 1; i >= 0; i--)
            {
                DuplicatableNode modNode = modifierNodes.Find(p => p.node.name == baseNodes[i].node.name && p.sameNameIndex == baseNodes[i].sameNameIndex);
                if (modNode != null)
                {
                    if (modNode.node.HasValue("deleteNode"))
                    {
                        baseNodes.RemoveAt(i);
                    }
                    else
                    {
                        modNode.node.CopyToAsModifier(baseNodes[i].node);
                    }
                    modifierNodes.Remove(modNode);
                }
            }

            // Now that the modifier node list only contains added nodes, insert them.
            for (int i = 0; i < modifierNodes.Count; i++)
            {
                baseNodes.Insert(baseNodes.FindLastIndex(p => p.node.name == modifierNodes[i].node.name) + 1, modifierNodes[i]);
            }

            // Rebuild the ConfigNode nodes
            baseNode.nodes.Clear();
            for (int i = 0; i < baseNodes.Count; i++)
            {
                baseNode.nodes.Add(baseNodes[i].node);
            }
        }
    }
}
