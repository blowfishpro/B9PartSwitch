using System;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class NodeObjectWrapperIConfigNode : INodeObjectWrapper
    {
        public readonly Type type;

        public NodeObjectWrapperIConfigNode(Type type)
        {
            type.ThrowIfNullArgument(nameof(type));
            if (!type.Implements<IConfigNode>()) throw new ArgumentException($"Type {type} does not implement {nameof(IConfigNode)}", nameof(type));
            this.type = type;
        }

        public void Load(ref object obj, ConfigNode node, OperationContext context)
        {
            obj.EnsureArgumentType<IConfigNode>(nameof(obj));
            node.ThrowIfNullArgument(nameof(node));

            if (obj.IsNull()) obj = Activator.CreateInstance(type);

            ((IConfigNode)obj).Load(node);
        }

        public ConfigNode Save(object obj, OperationContext context)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            obj.EnsureArgumentType<IConfigNode>(nameof(obj));

            ConfigNode node = new ConfigNode();
            ((IConfigNode)obj).Save(node);
            return node;
        }
    }
}
