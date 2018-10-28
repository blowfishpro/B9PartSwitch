using System;
using B9PartSwitch.Fishbones.Parsers;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public class NodeScalarMapper : INodeDataMapper
    {
        public readonly string name;
        public readonly INodeObjectWrapper nodeObjectWrapper;

        public NodeScalarMapper(string name, INodeObjectWrapper nodeObjectWrapper)
        {
            name.ThrowIfNullArgument(nameof(name));
            this.name = name;

            nodeObjectWrapper.ThrowIfNullArgument(nameof(nodeObjectWrapper));
            this.nodeObjectWrapper = nodeObjectWrapper;
        }

        public bool Load(ref object fieldValue, ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            ConfigNode innerNode = node.GetNode(name);
            if (innerNode.IsNull()) return false;

            nodeObjectWrapper.Load(ref fieldValue, innerNode, context);
             return true;
        }

        public bool Save(object fieldValue, ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            if (fieldValue.IsNull()) return false;

            ConfigNode innerNode = nodeObjectWrapper.Save(fieldValue, context);

            node.AddNode(name, innerNode);
            return true;
        }
    }
}
