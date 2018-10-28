using System;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class NodeObjectWrapperIContextualNode : INodeObjectWrapper
    {
        public readonly Type type;
        public NodeObjectWrapperIContextualNode(Type type)
        {
            type.ThrowIfNullArgument(nameof(type));
            if (!type.Implements<IContextualNode>()) throw new ArgumentException($"Type {type} does not implement {typeof(IContextualNode)}", nameof(type));
            this.type = type;
        }

        public void Load(ref object obj, ConfigNode node, OperationContext context)
        {
            obj.EnsureArgumentType<IContextualNode>(nameof(obj));
            node.ThrowIfNullArgument(nameof(node));
            context.ThrowIfNullArgument(nameof(context));

            if (obj.IsNull()) obj = Activator.CreateInstance(type);

            ((IContextualNode)obj).Load(node, context);
        }

        public ConfigNode Save(object obj, OperationContext context)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            obj.EnsureArgumentType<IContextualNode>(nameof(obj));
            context.ThrowIfNullArgument(nameof(context));

            ConfigNode node = new ConfigNode();
            ((IContextualNode)obj).Save(node, context);
            return node;
        }
    }
}
