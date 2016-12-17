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

        public bool Load(ConfigNode node, ref object fieldValue)
        {
            node.ThrowIfNullArgument(nameof(node));
            if (fieldValue.IsNotNull() && !(fieldValue is IConfigNode)) throw new ArgumentException($"{nameof(fieldValue)} must be of type '{nameof(IConfigNode)}' (got '{fieldValue.GetType()}')");

            ConfigNode innerNode = node.GetNode(name);
            if (innerNode.IsNull()) return false;

            if (fieldValue.IsNull()) fieldValue = Activator.CreateInstance(fieldType);

            ((IConfigNode)fieldValue).Load(innerNode);

            return true;
        }

        public bool Save(ConfigNode node, ref object fieldValue)
        {
            node.ThrowIfNullArgument(nameof(node));
            if (fieldValue.IsNotNull() && !(fieldValue is IConfigNode)) throw new ArgumentException($"{nameof(fieldValue)} must be of type '{nameof(IConfigNode)}' (got '{fieldValue.GetType().ToString()}')");

            if (fieldValue.IsNull()) return false;

            ConfigNode innerNode = new ConfigNode(name);
            ((IConfigNode)fieldValue).Save(innerNode);

            node.AddNode(innerNode);

            return true;
        }
    }
}
