using System;
using System.Collections;
using System.Collections.Generic;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public class NodeListMapper : INodeDataMapper
    {
        private readonly string name;
        
        private readonly Type elementType;
        private readonly Type listType;

        public NodeListMapper(string name, Type elementType)
        {
            name.ThrowIfNullArgument(nameof(name));
            elementType.ThrowIfNullArgument(nameof(elementType));

            if (!elementType.Implements<IConfigNode>()) throw new ArgumentException($"Must implement {typeof(IConfigNode)}", nameof(elementType));

            this.name = name;
            this.elementType = elementType;
            listType = typeof(List<>).MakeGenericType(elementType);
        }

        public bool Load(ConfigNode node, ref object fieldValue, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            fieldValue.EnsureArgumentType(listType, nameof(fieldValue));

            ConfigNode[] nodes = node.GetNodes(name);
            if (nodes.IsNullOrEmpty()) return false;

            if (fieldValue.IsNull()) fieldValue = Activator.CreateInstance(listType);
            IList list = (IList)fieldValue;

            foreach (ConfigNode innerNode in nodes)
            {
                if (innerNode.IsNull()) continue;
                IConfigNode obj = (IConfigNode)Activator.CreateInstance(elementType);
                obj.Load(innerNode);
                list.Add(obj);
            }

            return true;
        }

        public bool Save(ConfigNode node, object fieldValue, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            fieldValue.EnsureArgumentType(listType, nameof(fieldValue));

            IList list = (IList)fieldValue;
            if (list.IsNullOrEmpty()) return false;

            foreach (IConfigNode value in list)
            {
                if (value.IsNull()) continue;
                ConfigNode innerNode = new ConfigNode();
                value.Save(innerNode);
                node.AddNode(name, innerNode);
            }

            return true;
        }
    }
}
