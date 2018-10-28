using System;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public class NodeListMapperBuilder : INodeDataMapperBuilder
    {
        public readonly string nodeDataName;
        public readonly Type elementType;

        public NodeListMapperBuilder(string nodeDataName, Type fieldType)
        {
            nodeDataName.ThrowIfNullArgument(nameof(nodeDataName));
            fieldType.ThrowIfNullArgument(nameof(fieldType));

            this.nodeDataName = nodeDataName;

            if (fieldType.IsListType()) elementType = fieldType.GetGenericArguments()[0];
        }

        public bool CanBuild => elementType.IsNotNull() && NodeObjectWrapper.IsNodeType(elementType);

        public INodeDataMapper BuildMapper()
        {
            if (!CanBuild) throw new InvalidOperationException();

            return new NodeListMapper(nodeDataName, elementType, NodeObjectWrapper.For(elementType));
        }
    }
}
