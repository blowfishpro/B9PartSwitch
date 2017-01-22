using System;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public class ValueListMapperBuilder : INodeDataMapperBuilder
    {
        public readonly string nodeDataName;
        public readonly Type elementType;
        public readonly IValueParseMap parseMap;

        public ValueListMapperBuilder(string nodeDataName, Type fieldType, IValueParseMap parseMap)
        {
            nodeDataName.ThrowIfNullArgument(nameof(nodeDataName));
            fieldType.ThrowIfNullArgument(nameof(fieldType));
            parseMap.ThrowIfNullArgument(nameof(parseMap));

            this.nodeDataName = nodeDataName;
            this.parseMap = parseMap;

            if (fieldType.IsListType()) elementType = fieldType.GetGenericArguments()[0];
        }

        public bool CanBuild => elementType.IsNotNull() && parseMap.CanParse(elementType);

        public INodeDataMapper BuildMapper()
        {
            if (!CanBuild) throw new InvalidOperationException();

            return new ValueListMapper(nodeDataName, parseMap.GetParser(elementType));
        }
    }
}
