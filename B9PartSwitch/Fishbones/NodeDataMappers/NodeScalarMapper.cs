using System;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public class NodeScalarMapper : INodeDataMapper
    {
        private readonly string name;
        private readonly Type fieldType;

        public NodeScalarMapper(string name, Type fieldType)
        {
            name.ThrowIfNullArgument(nameof(name));
            fieldType.ThrowIfNullArgument(nameof(fieldType));

            this.name = name;
            this.fieldType = fieldType;
        }

        public bool Load(ref object fieldValue, ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            fieldValue.EnsureArgumentType<IConfigNode, IContextualNode>(nameof(fieldValue));

            ConfigNode innerNode = node.GetNode(name);
            if (innerNode.IsNull()) return false;

            if (fieldValue.IsNull()) fieldValue = Activator.CreateInstance(fieldType);

            NodeObjectWrapper.Load(fieldValue, innerNode, context);

            return true;
        }

        public bool Save(object fieldValue, ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            fieldValue.EnsureArgumentType<IConfigNode, IContextualNode>(nameof(fieldValue));

            if (fieldValue.IsNull()) return false;

            ConfigNode innerNode = new ConfigNode();
            NodeObjectWrapper.Save(fieldValue, innerNode, context);

            node.AddNode(name, innerNode);

            return true;
        }
    }
}
