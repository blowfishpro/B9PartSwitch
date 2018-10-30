using System;
using B9PartSwitch.Fishbones.Context;

namespace B9PartSwitch.Fishbones.Parsers
{
    public class NodeObjectWrapperConfigNode : INodeObjectWrapper
    {
        public void Load(ref object obj, ConfigNode node, OperationContext context)
        {
            node.ThrowIfNullArgument(nameof(node));
            obj = node.CreateCopy();
        }

        public ConfigNode Save(object obj, OperationContext context)
        {
            obj.ThrowIfNullArgument(nameof(obj));
            obj.EnsureArgumentType<ConfigNode>(nameof(obj));
            return ((ConfigNode)obj).CreateCopy();
        }
    }
}
