using System;
using B9PartSwitch.Fishbones.Parsers;

namespace B9PartSwitch.Fishbones.NodeDataMappers
{
    public class ValueScalarMapperBuilder : INodeDataMapperBuilder
    {
        public readonly string nodeDataName;
        public readonly Type fieldType;
        public readonly IValueParseMap parseMap;

        public ValueScalarMapperBuilder(string nodeDataName, Type fieldType, IValueParseMap parseMap)
        {
            nodeDataName.ThrowIfNullArgument(nameof(nodeDataName));
            fieldType.ThrowIfNullArgument(nameof(fieldType));
            parseMap.ThrowIfNullArgument(nameof(parseMap));

            this.nodeDataName = nodeDataName;
            this.fieldType = fieldType;
            this.parseMap = parseMap;
        }

        public bool CanBuild => parseMap.CanParse(fieldType);

        public INodeDataMapper BuildMapper()
        {
            if (!CanBuild) throw new InvalidOperationException();

            return new ValueScalarMapper(nodeDataName, parseMap.GetParser(fieldType));
        }
    }
}
