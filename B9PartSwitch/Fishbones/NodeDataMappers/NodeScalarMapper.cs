using System;
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

        public bool Load(ConfigNode node, ref object fieldValue, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            fieldValue.EnsureArgumentType<IConfigNode>(nameof(fieldValue));

            ConfigNode innerNode = node.GetNode(name);
            if (innerNode.IsNull()) return false;

            if (fieldValue.IsNull()) fieldValue = Activator.CreateInstance(fieldType);

            ((IConfigNode)fieldValue).Load(innerNode);

            return true;
        }

        public bool Save(ConfigNode node, object fieldValue, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            fieldValue.EnsureArgumentType<IConfigNode>(nameof(fieldValue));

            if (fieldValue.IsNull()) return false;

            ConfigNode innerNode = new ConfigNode();
            ((IConfigNode)fieldValue).Save(innerNode);

            node.AddNode(name, innerNode);

            return true;
        }
    }
}
