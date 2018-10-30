using System;

namespace B9PartSwitch.Fishbones.Parsers
{
    public static class NodeObjectWrapper
    {
        public static INodeObjectWrapper For(Type type)
        {
            type.ThrowIfNullArgument(nameof(type));
            if (type.Implements<IConfigNode>())
                return new NodeObjectWrapperIConfigNode(type);
            else if (type.Implements<IContextualNode>())
                return new NodeObjectWrapperIContextualNode(type);
            else if (type == typeof(ConfigNode))
                return new NodeObjectWrapperConfigNode();
            else
                throw new NotImplementedException($"No way to build node object wrapper for type {type}");
        }

        public static bool IsNodeType(Type type)
        {
            return type.Implements<IConfigNode>() || type.Implements<IContextualNode>() || type == typeof(ConfigNode);
        }
    }
}
