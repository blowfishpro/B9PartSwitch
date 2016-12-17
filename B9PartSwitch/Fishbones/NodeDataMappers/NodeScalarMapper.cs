using System;

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

        public bool Load(ConfigNode node, ref object result)
        {
            node.ThrowIfNullArgument(nameof(node));
            if (result.IsNotNull() && !(result is IConfigNode)) throw new ArgumentException($"{nameof(result)} must be of type '{nameof(IConfigNode)}' (got '{result.GetType()}')");

            ConfigNode innerNode = node.GetNode(name);
            if (innerNode.IsNull()) return false;

            if (result.IsNull()) result = Activator.CreateInstance(fieldType);

            ((IConfigNode)result).Load(innerNode);

            return true;
        }

        public bool Save(ConfigNode node, ref object input)
        {
            node.ThrowIfNullArgument(nameof(node));
            if (input.IsNotNull() && !(input is IConfigNode)) throw new ArgumentException($"{nameof(input)} must be of type '{nameof(IConfigNode)}' (got '{input.GetType().ToString()}')");

            if (input.IsNull()) return false;

            ConfigNode innerNode = new ConfigNode(name);
            ((IConfigNode)input).Save(innerNode);

            node.AddNode(innerNode);

            return true;
        }
    }
}
